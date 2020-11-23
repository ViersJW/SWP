﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using Radzen.Blazor;
using SWP.Application.LegalSwp.CashMovements;
using SWP.Application.LegalSwp.Clients;
using SWP.Application.LegalSwp.TimeRecords;
using SWP.UI.BlazorApp.LegalApp.Stores.Main;
using SWP.UI.Components.LegalSwpBlazorComponents.Dialogs;
using SWP.UI.Components.LegalSwpBlazorComponents.ViewModels.Data;
using SWP.UI.Components.LegalSwpBlazorComponents.ViewModels.Data.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP.UI.BlazorApp.LegalApp.Stores.MyApp
{

    public class MyAppState
    {
        public List<CategoryDataItem> ClientsCases { get; set; } = new List<CategoryDataItem>();
        public List<ClientData> ProductivityData { get; set; } = new List<ClientData>();
        public double TotalBalance => ProductivityData.Count != 0 ? ProductivityData.Sum(x => x.DataByDate.Sum(y => y.Number)) : 0;
        public double TotalExpenses => ProductivityData.Count != 0 ? ProductivityData.Sum(x => x.DataByDate.Sum(y => y.Expenses)) : 0;
        public IdentityUser SelectedUser { get; set; }
        public ColorScheme ColorScheme { get; set; } = ColorScheme.Pastel;
    }

    [UIScopedService]
    public class MyAppStore : StoreBase<MyAppState>
    {
        private readonly GeneralViewModel _generalViewModel;
        public MainStore MainStore => _serviceProvider.GetRequiredService<MainStore>();

        public MyAppStore(
            IServiceProvider serviceProvider,
            IActionDispatcher actionDispatcher,
            NotificationService notificationService, 
            DialogService dialogService,
            GeneralViewModel generalViewModel) 
            : base(serviceProvider, actionDispatcher, notificationService, dialogService)
        {
            _generalViewModel = generalViewModel;
        }

        public Task Initialize()
        {
            RefreshSore();
            return Task.CompletedTask;
        }

        private void RefreshClientCases()
        {
            _state.ClientsCases.Clear();

            using var scope = _serviceProvider.CreateScope();
            var getClients = scope.ServiceProvider.GetRequiredService<GetClients>();

            foreach (var client in MainStore.GetState().Clients)
            {
                _state.ClientsCases.Add(new CategoryDataItem
                {
                    Category = client.Name,
                    Number = getClients.CountCasesPerClient(client.Id)
                });
            }
        }

        private void RefreshSpecificProductivityData()
        {
            _state.ProductivityData.Clear();

            using var scope = _serviceProvider.CreateScope();
            var getTimeRecords = scope.ServiceProvider.GetRequiredService<GetTimeRecords>();
            var getCashMovements = scope.ServiceProvider.GetRequiredService<GetCashMovements>();

            var dataSet = new ClientData
            {
                Name = MainStore.GetState().ActiveClient.Name
            };

            var timeRecords = getTimeRecords.Get(MainStore.GetState().ActiveClient.Id);
            var cashMovements = getCashMovements.Get(MainStore.GetState().ActiveClient.Id);

            for (int i = 0; i < 13; i++)
            {
                var date = new DateTime(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month));
                var records = timeRecords.Where(x => x.EventDate <= date).ToList();

                dataSet.DataByDate.Add(new DateDataItem
                {
                    Date = date,
                    Time = new TimeSpan(timeRecords.Where(x => x.EventDate <= date).Sum(x => x.Hours), timeRecords.Where(x => x.EventDate <= date).Sum(x => x.Minutes), 0),
                    Number = Math.Round(cashMovements.Where(x => x.EventDate <= date && !x.Expense).Sum(x => x.Amount), 2),
                    Expenses = Math.Abs(Math.Round(cashMovements.Where(x => x.EventDate <= date && x.Expense).Sum(x => x.Amount), 2)),
                });
            }

            _state.ProductivityData.Add(dataSet);
        }

        private void RefreshAllProductivityData()
        {
            _state.ProductivityData.Clear();

            using var scope = _serviceProvider.CreateScope();
            var getTimeRecords = scope.ServiceProvider.GetRequiredService<GetTimeRecords>();
            var getCashMovements = scope.ServiceProvider.GetRequiredService<GetCashMovements>();

            foreach (var client in MainStore.GetState().Clients)
            {
                var dataSet = new ClientData
                {
                    Name = client.Name
                };

                var timeRecords = getTimeRecords.Get(client.Id);
                var cashMovements = getCashMovements.Get(client.Id);

                for (int i = 0; i < 13; i++)
                {
                    var date = new DateTime(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month, DateTime.DaysInMonth(DateTime.Now.AddMonths(-i).Year, DateTime.Now.AddMonths(-i).Month));
                    var records = timeRecords.Where(x => x.EventDate <= date).ToList();

                    dataSet.DataByDate.Add(new DateDataItem
                    {
                        Date = date,
                        Time = new TimeSpan(timeRecords.Where(x => x.EventDate <= date).Sum(x => x.Hours), timeRecords.Where(x => x.EventDate <= date).Sum(x => x.Minutes), 0),
                        Number = Math.Round(cashMovements.Where(x => x.EventDate <= date && !x.Expense).Sum(x => x.Amount), 2),
                        Expenses = Math.Abs(Math.Round(cashMovements.Where(x => x.EventDate <= date && x.Expense).Sum(x => x.Amount), 2)),
                    });
                }

                _state.ProductivityData.Add(dataSet);
            }
        }

        public void SelectedUserChange(object value)
        {
            var input = (string)value;
            if (value != null)
            {
                _state.SelectedUser = MainStore.GetState().User.RelatedUsers.FirstOrDefault(x => x.Id == input);
            }
            else
            {
                _state.SelectedUser = null;
            }
        }

        public async Task RemoveRelation()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var profileClaim = MainStore.GetState().User.ProfileClaim;
                var selectedUser = MainStore.GetState().User.RelatedUsers.FirstOrDefault(x => x.Id == _state.SelectedUser.Id);
                var result = await userManager.RemoveClaimAsync(selectedUser, profileClaim);

                _state.SelectedUser = MainStore.GetState().User.User;
                await MainStore.RefreshRelatedUsers();
            }
            catch (Exception ex)
            {
                await MainStore.ShowErrorPage(ex);
            }
        }

        public async Task RemoveProfileData()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var deleteClient = scope.ServiceProvider.GetRequiredService<DeleteClient>();

                await deleteClient.Delete(MainStore.GetState().User.Profile);

                ShowNotification(NotificationSeverity.Success, "Sukces!", $"Usunięto wszystkie dane powiązane z profilem: {MainStore.GetState().User.Profile}", GeneralViewModel.NotificationDuration);
            }
            catch (Exception ex)
            {
                await MainStore.ShowErrorPage(ex);
            }
        }

        public void ConfirmRemoveRelation()
        {
            _dialogService.Open<GenericDialogPopup>("Order TEST",
                new Dictionary<string, object>()
                {
                    { "Title", "TEST Title Passed" },
                    { "TaskToExecuteAsync", new Func<Task>(MainStore.ThrowTestException) },
                    { "Description", "This is sample Description" },
                },
                _generalViewModel.DefaultDialogOptions);
        }

        protected override void HandleActions(IAction action)
        {
    
        }

        public override void CleanUpStore()
        {
            _state.SelectedUser = null;
        }

        public override void RefreshSore()
        {
            if (MainStore.GetState().ActiveClient != null)
            {
                RefreshClientCases();
                RefreshSpecificProductivityData();
            }
            else
            {
                RefreshClientCases();
                RefreshAllProductivityData();
            }
        }
    }
}
