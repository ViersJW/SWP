﻿using Microsoft.AspNetCore.Components;
using SWP.UI.BlazorApp;
using SWP.UI.BlazorApp.LegalApp.Stores.Archive;
using SWP.UI.BlazorApp.LegalApp.Stores.Archive.Actions;
using SWP.UI.BlazorApp.LegalApp.Stores.Enums;
using SWP.UI.BlazorApp.LegalApp.Stores.Main;
using System;

namespace SWP.UI.Components.LegalSwpBlazorComponents
{
    public partial class LegalSwpArchive : IDisposable
    {
        [Inject]
        public MainStore MainStore { get; set; }
        [Inject]
        public ArchiveStore ArchiveStore { get; set; }
        [Inject]
        public IActionDispatcher ActionDispatcher { get; set; }

        public string ArchvizedClientsFilterValue;

        public void Dispose()
        {
            MainStore.RemoveStateChangeListener(RefreshView);
            ArchiveStore.RemoveStateChangeListener(RefreshView);
            ArchiveStore.CleanUpStore();
        }

        private void RefreshView()
        {
            if (MainStore.GetState().ActiveClient == null)
            {
                MainStore.SetActivePanel(LegalAppPanels.MyApp);
                return;
            }

            ArchiveStore.RefreshData();
            StateHasChanged();
        }

        protected override void OnInitialized()
        {
            MainStore.AddStateChangeListener(RefreshView);
            ArchiveStore.AddStateChangeListener(RefreshView);
            ArchiveStore.Initialize();
        }

        public bool addClientformVisible = false;

        public void ShowHideClientFormI() => addClientformVisible = !addClientformVisible;

        private void RecoverSelectedClient() => ActionDispatcher.Dispatch(new RecoverSelectedClientAction());

        private void DeleteSelectedClient() => ActionDispatcher.Dispatch(new DeleteSelectedClientAction());

        private void RecoverSelectedCase() => ActionDispatcher.Dispatch(new RecoverSelectedCaseAction());

        private void DeleteSelectedCase() => ActionDispatcher.Dispatch(new DeleteSelectedCaseAction());

        private void SelectedArchivizedClientChange(object client) => ActionDispatcher.Dispatch(new SelectedArchivizedClientChangeAction { Client = client });

        private void SelectedArchivizedCaseChange(object c) => ActionDispatcher.Dispatch(new SelectedArchivizedCaseChangeAction { Case = c });
    }
}
