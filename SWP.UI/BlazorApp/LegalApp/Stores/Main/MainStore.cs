﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using SWP.Application.LegalSwp.Cases;
using SWP.Application.LegalSwp.Clients;
using SWP.Application.LegalSwp.Jobs;
using SWP.Application.Log;
using SWP.Domain.Models.SWPLegal;
using SWP.UI.BlazorApp.LegalApp.Stores.Enums;
using SWP.UI.BlazorApp.LegalApp.Stores.Error;
using SWP.UI.Components.LegalSwpBlazorComponents.ViewModels.Data;
using SWP.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWP.UI.BlazorApp.LegalApp.Stores.Main
{
    public class MainState
    {
        public string ActiveUserId { get; set; }
        public UserModel User { get; set; } = new UserModel();
        public List<ClientViewModel> Clients { get; set; } = new List<ClientViewModel>();
        public ClientViewModel ActiveClient { get; set; }
        public LegalAppPanels ActivePanel { get; set; } = LegalAppPanels.MyApp;
        public string SelectedClientString { get; set; }
    }

    [UIScopedService]
    public class MainStore : StoreBase
    {
        private readonly MainState _state;
        private readonly ErrorStore _errorStore;

        public MainState GetState() => _state;

        public MainStore(IServiceProvider serviceProvider, IActionDispatcher actionDispatcher, NotificationService notificationService, ErrorStore errorStore) : base(serviceProvider, actionDispatcher, notificationService)
        {
            _state = new MainState();
            _errorStore = errorStore;
        }

        public async Task InitializeState(string userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                _state.ActiveUserId = userId;

                _state.User.User = await userManager.FindByIdAsync(_state.ActiveUserId);
                _state.User.Claims = await userManager.GetClaimsAsync(_state.User.User) as List<Claim>;
                _state.User.Roles = await userManager.GetRolesAsync(_state.User.User) as List<string>;
                _state.User.RelatedUsers = await userManager.GetUsersForClaimAsync(_state.User.ProfileClaim);

                ReloadClientsDrop();
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                var createLogRecord = scope.ServiceProvider.GetRequiredService<CreateLogRecord>();

                await createLogRecord.Create(new CreateLogRecord.Request
                {
                    Message = ex.Message,
                    UserId = _state.ActiveUserId,
                    StackTrace = ex.StackTrace
                });
            }
            finally
            {
                BroadcastStateChange();
            }
        }

        private void ReloadClientsDrop()
        {
            using var scope = _serviceProvider.CreateScope();
            var getClients = scope.ServiceProvider.GetRequiredService<GetClients>();

            _state.Clients = getClients.GetClientsWithoutData(_state.User.Profile)?.Select(x => (ClientViewModel)x).ToList();
        }

        public void SetActivePanel(LegalAppPanels panel) => _state.ActivePanel = panel;

        public async Task ActiveClientChange(object value)
        {
            EnableLoading("Wczytywanie Klienta...");

            if (value != null)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var getClient = scope.ServiceProvider.GetRequiredService<GetClient>();

                    _state.ActiveClient = getClient.GetCleanClient(int.Parse(value.ToString()));
                }
                catch (Exception ex)
                {
                    await ShowErrorPage(ex);
                }
                finally
                {
                    DisableLoading();
                }
            }
            else
            {
                if (_state.ActivePanel != LegalAppPanels.Calendar)
                {
                    SetActivePanel(LegalAppPanels.MyApp);
                }

                _state.ActiveClient = null;

                DisableLoading();
            }
        }

        public async Task ReloadActiveClient()
        {
            EnableLoading("Wczytywanie Klienta...");

            if (_state.ActiveClient == null)
            {
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var getClient = scope.ServiceProvider.GetRequiredService<GetClient>();

                _state.ActiveClient = getClient.Get(_state.ActiveClient.Id);
            }
            catch (Exception ex)
            {
                await ShowErrorPage(ex);
            }
            finally
            {
                DisableLoading();
            }
        }

        public async Task ShowErrorPage(Exception ex)
        {
            await _errorStore.SetException(ex, _state.ActiveUserId);
            _state.ActivePanel = LegalAppPanels.ErrorPage;
            BroadcastStateChange();
        }

        public void DismissErrorPage()
        {
            _state.ActivePanel = LegalAppPanels.MyApp;
            _state.ActiveClient = null;
            _state.SelectedClientString = null;
            ReloadClientsDrop();
            BroadcastStateChange();
        }

        public async Task RefreshRelatedUsers()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                _state.User.RelatedUsers = await userManager.GetUsersForClaimAsync(_state.User.ProfileClaim);
                BroadcastStateChange();
            }
            catch (Exception ex)
            {
                await ShowErrorPage(ex);
            }
        }

        public Task ThrowTestException()
        {
            try
            {
                throw new Exception("bablabla");
            }
            catch (Exception ex)
            {
                return ShowErrorPage(ex);
            }
        }

        public void UpdateClientsList(ClientViewModel input) => _state.Clients[_state.Clients.FindIndex(x => x.Id == input.Id)] = input;

        public void RefreshClients()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var getClients = scope.ServiceProvider.GetRequiredService<GetClients>();

                _state.Clients = getClients.GetClientsWithoutData(_state.User.Profile, true).Select(x => (ClientViewModel)x).ToList();
                _state.ActiveClient = null;
                _state.SelectedClientString = null;
                BroadcastStateChange();
            }
            catch (Exception ex)
            {
                ShowErrorPage(ex).GetAwaiter();
            }
        }

        public void AddClient(ClientViewModel input) => _state.Clients.Add(input);

        public void RemoveClient(int id)
        {
            _state.Clients.RemoveAll(x => x.Id == id);

            if (_state.ActiveClient?.Id == id)
            {
                _state.ActiveClient = null;
                _state.SelectedClientString = null;
            }

            BroadcastStateChange();
        }

        public ClientViewModel GetActiveClient() => _state.ActiveClient;

        public UserModel GetApplicationUser() => _state.User;

        public void RefreshActiveClientData()
        {
            try
            {
                if (_state.ActiveClient != null)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var getClient = scope.ServiceProvider.GetRequiredService<GetClient>();

                    ClientViewModel newModel = getClient.Get(_state.ActiveClient.Id);

                    _state.ActiveClient = newModel;
                }

                BroadcastStateChange();
            }
            catch (Exception ex)
            {
                ShowErrorPage(ex).GetAwaiter();
            }
        }

        protected override void HandleActions(IAction action)
        {
            throw new NotImplementedException();
        }

        public override void CleanUpStore()
        {
            _state.SelectedClientString = null;
        }
    }
}
