﻿namespace MoneySaver.Services.Data.Models.Records
{
    public class RecordInfoDto
    {
        public string Id { get; set; }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public string Category { get; set; }

        public string Type { get; set; }

        public string Wallet { get; set; }

        public string CreatedOn { get; set; }
    }
}