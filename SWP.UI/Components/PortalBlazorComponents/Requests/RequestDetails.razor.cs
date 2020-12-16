﻿using Microsoft.AspNetCore.Components;
using SWP.UI.BlazorApp;
using SWP.UI.BlazorApp.PortalApp.Stores.Requests;
using SWP.UI.BlazorApp.PortalApp.Stores.Requests.RequestsPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP.UI.Components.PortalBlazorComponents.Requests
{
    public partial class RequestDetails : IDisposable
    {
        [Inject]
        public RequestsPanelDetailsStore Store { get; set; }
        [Inject]
        public RequestsMainPanelStore MainStore { get; set; }
        [Inject]
        public IActionDispatcher ActionDispatcher { get; set; }

        public void Dispose()
        {
            MainStore.RemoveStateChangeListener(UpdateView);
            Store.RemoveStateChangeListener(UpdateView);
        }

        private void UpdateView()
        {
            Store.RefreshSore();
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            Store.Initialize();
            MainStore.AddStateChangeListener(UpdateView);
            Store.AddStateChangeListener(UpdateView);
        }

        #region Actions







        #endregion
    }
}
