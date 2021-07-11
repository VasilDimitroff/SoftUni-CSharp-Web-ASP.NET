﻿namespace MoneySaver.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using MoneySaver.Common;
    using MoneySaver.Data;
    using MoneySaver.Data.Models;
    using MoneySaver.Data.Models.Enums;
    using MoneySaver.Services.Data.Contracts;

    public class TradesService : ITradesService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ICompaniesService companiesService;

        public TradesService(
            ApplicationDbContext dbContext,
            ICompaniesService companiesService
            )
        {
            this.dbContext = dbContext;
            this.companiesService = companiesService;
        }

        public async Task CreateBuyTradeAsync(string userId, int investmentWalletId, string companyTicker, int quantity, decimal pricePerShare)
        {
            if (!await this.CanUserEditInvestmentWallet(userId, investmentWalletId))
            {
                throw new ArgumentException(GlobalConstants.CannotEditInvestmentWallet);
            }

            var company = await this.companiesService.GetCompanyByTickerAsync(companyTicker);

            if (pricePerShare > 0)
            {
                pricePerShare *= -1;
            }

            var userTrade = new Trade()
            {
                Id = Guid.NewGuid().ToString(),
                InvestmentWalletId = investmentWalletId,
                CreatedOn = DateTime.UtcNow,
                Company = company,
                CompanyTicker = company.Ticker,
                Price = pricePerShare,
                StockQuantity = quantity,
                Type = TradeType.Buy,
            };

            await this.dbContext.Trades.AddAsync(userTrade);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task CreateSellTradeAsync(string userId, int investmentWalletId, string companyTicker, int quantity, decimal pricePerShare)
        {
            if (!await this.CanUserEditInvestmentWallet(userId, investmentWalletId))
            {
                throw new ArgumentException(GlobalConstants.CannotEditInvestmentWallet);
            }

            var company = await this.companiesService.GetCompanyByTickerAsync(companyTicker);

            if (pricePerShare < 0)
            {
                pricePerShare *= -1;
            }

            int currentlyHoldings = this.GetCompanyStocksHoldingsCount(companyTicker, quantity, investmentWalletId);

            if (currentlyHoldings < quantity)
            {
                throw new ArgumentException(GlobalConstants.NotEnoughQuantity);
            }

            var userTrade = new Trade()
            {
                Id = Guid.NewGuid().ToString(),

                CreatedOn = DateTime.UtcNow,
                Company = company,
                CompanyTicker = company.Ticker.ToUpper(),
                Price = pricePerShare,
                StockQuantity = quantity,
                Type = TradeType.Sell,
                InvestmentWalletId = investmentWalletId,
            };

            await this.dbContext.Trades.AddAsync(userTrade);
            await this.dbContext.SaveChangesAsync();
        }

        public int GetCompanyStocksHoldingsCount(string companyTicker, int quantity, int investmentWalletId)
        {
            int totalBuyQuantity = this.dbContext.Trades
                .Where(ut => ut.InvestmentWalletId == investmentWalletId
                    && ut.CompanyTicker == companyTicker
                    && ut.Type == TradeType.Buy)
                .Sum(ut => ut.StockQuantity);

            int totalSellQuantity = this.dbContext.Trades
                .Where(ut => ut.InvestmentWalletId == investmentWalletId
                    && ut.CompanyTicker == companyTicker
                    && ut.Type == TradeType.Sell)
                .Sum(ut => ut.StockQuantity);

            int currentlyHoldingQuantity = totalBuyQuantity - totalSellQuantity;

            return currentlyHoldingQuantity;
        }

        private async Task<bool> CanUserEditInvestmentWallet(string userId, int investmentWalletId)
        {
            var investmentWallet = await this.dbContext.InvestmentWallets
                .Where(w => w.Id == investmentWalletId && w.ApplicationUserId == userId)
                .FirstOrDefaultAsync();

            if (investmentWallet == null)
            {
                return false;
            }

            return true;
        }
    }
}