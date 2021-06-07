﻿namespace MoneySaver.Web.Areas.Administration.Controllers
{
    using MoneySaver.Services.Data;
    using MoneySaver.Web.ViewModels.Administration.Dashboard;

    using Microsoft.AspNetCore.Mvc;
    using MoneySaver.Services.Data.Contracts;

    public class DashboardController : AdministrationController
    {
        private readonly ISettingsService settingsService;

        public DashboardController(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public IActionResult Index()
        {
            var viewModel = new IndexViewModel { SettingsCount = this.settingsService.GetCount(), };
            return this.View(viewModel);
        }
    }
}