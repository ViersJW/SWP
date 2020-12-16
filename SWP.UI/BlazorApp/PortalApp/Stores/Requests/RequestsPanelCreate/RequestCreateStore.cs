﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using SWP.Application.PortalCustomers.LicenseManagement;
using SWP.Application.PortalCustomers.RequestsManagement;
using SWP.Domain.Enums;
using SWP.Domain.Models.Portal;
using SWP.UI.BlazorApp.PortalApp.Stores.Requests.RequestsPanel;
using SWP.UI.BlazorApp.PortalApp.Stores.Requests.RequestsPanelCreate.Actions;
using SWP.UI.Components.PortalBlazorComponents.Requests.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP.UI.BlazorApp.PortalApp.Stores.Requests.RequestsPanelCreate
{
    public class RequestCreateState
    {
        public StepsConfiguration StepsConfig { get; set; } = new StepsConfiguration();
        public CreateRequest.Request NewRequest { get; set; } = new CreateRequest.Request();
        public List<UserLicense> UserLicenses { get; set; } = new List<UserLicense>();

        public class StepsConfiguration
        {
            public int ChosenRequestReason { get; set; } = 0;
            public int ChosenApplication { get; set; } = 0;
            public RequestReason NewRequestReason { get; set; } = RequestReason.Query;
            public ApplicationType NewRequestApplication { get; set; } = ApplicationType.NoApp;
            public string Message { get; set; }
        }
    }

    [UIScopedService]
    public class RequestCreateStore : StoreBase<RequestCreateState>
    {
        private readonly ILogger<RequestCreateStore> _logger;
        public RequestsMainPanelStore MainStore => _serviceProvider.GetRequiredService<RequestsMainPanelStore>();

        public RequestCreateStore(
            IServiceProvider serviceProvider,
            IActionDispatcher actionDispatcher,
            ILogger<RequestCreateStore> logger)
            : base(serviceProvider, actionDispatcher)
        {
            _logger = logger;
        }

        public void SetRelatedUsersNumber(int number) => _state.NewRequest.RelatedUsers = number;

        public void Initialize()
        {
            using var scope = _serviceProvider.CreateScope();
            var getLicense = scope.ServiceProvider.GetRequiredService<GetLicense>();

            _state.UserLicenses = getLicense.GetAll(MainStore.GetState().ActiveUserId);
        }

        protected override async void HandleActions(IAction action)
        {
            switch (action.Name)
            {
                case SelectedRequestReasonChangeAction.SelectedRequestSubjectChange:
                    var selectedRequestReasonChangeAction = (SelectedRequestReasonChangeAction)action;
                    SelectedRequestReasonChange(selectedRequestReasonChangeAction.Arg);
                    break;
                case CreateNewRequestAction.CreateNewRequest:
                    var createNewRequestAction = (CreateNewRequestAction)action;
                    await CreateNewRequest(createNewRequestAction.Arg);
                    break;
                case SelectedRequestApplicationChangeAction.SelectedRequestApplicationChange:
                    var selectedRequestApplicationChangeAction = (SelectedRequestApplicationChangeAction)action;
                    SelectedRequestApplicationChange(selectedRequestApplicationChangeAction.Arg);
                    break;
                default:
                    break;
            }
        }

        private void SelectedRequestReasonChange(object arg)
        {
            if (arg is null) return;

            _state.StepsConfig.ChosenRequestReason = (int)arg;
            _state.StepsConfig.NewRequestReason = (RequestReason)(int)arg;
        }

        private void SelectedRequestApplicationChange(object arg)
        {
            if (arg is null) return;

            _state.StepsConfig.ChosenApplication = (int)arg;
            _state.StepsConfig.NewRequestApplication = (ApplicationType)(int)arg;
        }

        private async Task CreateNewRequest(CreateRequest.Request request)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var createRequest = scope.ServiceProvider.GetRequiredService<CreateRequest>();

                await createRequest.Create(new CreateRequest.Request
                {
                    Application = (int)_state.StepsConfig.NewRequestApplication,
                    Created = DateTime.Now,
                    CreatedBy = MainStore.GetState().ActiveUserId,
                    EndDate = request.EndDate,
                    Reason = (int)_state.StepsConfig.NewRequestReason,
                    RelatedUsers = request.RelatedUsers,
                    RequestMessage = new CreateRequest.RequestMessage 
                    { 
                        AuthorId = MainStore.GetState().ActiveUserId, 
                        Created = DateTime.Now, 
                        CreatedBy = MainStore.GetState().ActiveUserId, 
                        Message = request.RequestMessage.Message 
                    },
                    RequestorId = MainStore.GetState().ActiveUserId,
                    StartDate = request.StartDate,
                    Status = (int)RequestStatus.WaitingForAnswer,
                });

                MainStore.RefreshSore();
                MainStore.SetActiveComponent(RequestsMainPanelState.InnerComponents.Info);
            }
            catch (Exception ex)
            {
                //MainStore.ShowErrorPage(ex);
            }
        }

        public override void CleanUpStore()
        {

        }

        public override void RefreshSore()
        {

        }
    }
}
