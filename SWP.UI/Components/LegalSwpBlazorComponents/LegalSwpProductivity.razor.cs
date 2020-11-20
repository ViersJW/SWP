﻿using Microsoft.AspNetCore.Components;
using Radzen;
using SWP.UI.BlazorApp.LegalApp.Services.Reporting;
using SWP.UI.BlazorApp.LegalApp.Stores.Main;
using SWP.UI.BlazorApp.LegalApp.Stores.Productivity;
using SWP.UI.Components.LegalSwpBlazorComponents.ViewModels.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP.UI.Components.LegalSwpBlazorComponents
{
    public partial class LegalSwpProductivity
    {
        [Inject]
        public MainStore MainStore { get; set; }
        [Inject]
        public ProductivityStore ProductivityStore { get; set; }
        [Inject]
        public GeneralViewModel Gvm { get; set; }
        [Inject]
        public TooltipService TooltipService { get; set; }

        public string ArchvizedClientsFilterValue;

        public void Dispose()
        {
            MainStore.RemoveStateChangeListener(UpdateView);
            ProductivityStore.RemoveStateChangeListener(UpdateView);
            ProductivityStore.CleanUpStore();
        }

        private void UpdateView() => StateHasChanged();

        protected override void OnInitialized()
        {
            MainStore.AddStateChangeListener(UpdateView);
            ProductivityStore.AddStateChangeListener(UpdateView);
            ProductivityStore.Initialize();
        }

        [Inject]
        public LegalTimeSheetReport LegalTimeSheetReport { get; set; }

        public bool showFirstSection = false;
        public void ShowHideFirstSection() => showFirstSection = !showFirstSection;

        public bool showSecondSection = false;
        public void ShowHideSecondSection() => showSecondSection = !showSecondSection;
        
        public bool showThirdSection = false;
        public void ShowHideThirdSection() => showThirdSection = !showThirdSection;

        public bool infoBoxVisibleI = false;
        public void ShowHideInfoBoxI() => infoBoxVisibleI = !infoBoxVisibleI;

        public bool infoBoxVisibleII = false;
        public void ShowHideInfoBoxII() => infoBoxVisibleII = !infoBoxVisibleII;

        public bool infoBoxVisibleIII = false;
        public void ShowHideInfoBoxIII() => infoBoxVisibleIII = !infoBoxVisibleIII;

        public async Task GenerateTimesheetReport(LegalTimeSheetReport.ReportData reportData)
        {
            try
            {
                var legalTimeSheetReport = LegalTimeSheetReport;

                var productivityRecords = new List<TimeRecordViewModel>();

                if (ProductivityStore.GetState().SelectedFont != null)
                {
                    reportData.FontName = ProductivityStore.GetState().SelectedFont.FontName;
                }
                else
                {
                    reportData.FontName = "Anonymous_Pro";
                }

                if (reportData.UseSelectedMonth)
                {
                    if (ProductivityStore.GetState().SelectedMonth != null)
                    {
                        var month = ProductivityStore.GetState().SelectedMonth.Month;
                        var year = ProductivityStore.GetState().SelectedMonth.Year;

                        productivityRecords = MainStore.GetState().ActiveClient.TimeRecords
                            .Where(x => x.EventDate.Month == month && x.EventDate.Year == year).ToList();

                        reportData.StartDate = new DateTime(year, month, 1);
                        reportData.EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                    }
                    else
                    {
                        MainStore.ShowNotification(NotificationSeverity.Warning, "Uwaga!", $"Najpierw przefiltruj dane po wybranym miesiącu", GeneralViewModel.NotificationDuration);
                        return;
                    }
                }
                else
                {
                    productivityRecords = MainStore.GetState().ActiveClient.TimeRecords
                        .Where(x => x.EventDate >= reportData.StartDate && x.EventDate <= reportData.EndDate).ToList();
                }

                if (productivityRecords.Count == 0)
                {
                    MainStore.ShowNotification(NotificationSeverity.Warning, "Uwaga!", $"Nie wykryto żadnych wpisów w wybranym przedziale dat", GeneralViewModel.NotificationDuration);
                    return;
                }

                reportData.ClientName = MainStore.GetState().ActiveClient.Name;
                reportData.Records = productivityRecords;
                reportData.ReportName = $"Rozliczenie_{DateTime.Now:yyyy-MM-dd-hh-mm-ss}";
                await legalTimeSheetReport.GeneratePDF(reportData);
            }
            catch (Exception ex)
            {
                await MainStore.ShowErrorPage(ex);
            }
        }
    }
}
