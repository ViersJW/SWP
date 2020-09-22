﻿using SWP.Domain.Infrastructure;
using SWP.Domain.Models.SWPLegal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SWP.Application.LegalSwp.CashMovements
{
    [TransientService]
    public class CreateCashMovement
    {
        private readonly ILegalSwpManager legalSwpManager;
        public CreateCashMovement(ILegalSwpManager legalSwpManager) => this.legalSwpManager = legalSwpManager;

        public Task<CashMovement> Create(int clientId, string profile, Request request) =>
            legalSwpManager.CreateCashMovement(clientId, profile, new CashMovement
            {
                Name = request.Name,
                Amount = request.ResultAmount,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UpdatedBy = request.UpdatedBy,
                CreatedBy = request.UpdatedBy
            });

        public class Request
        {
            public string Name { get; set; }
            public double Amount { get; set; }
            public int CashFlowDirection { get; set; }
            public string UpdatedBy { get; set; }

            public double ResultAmount => CashFlowDirection == 0 ? Math.Abs(Amount) * -1 : Math.Abs(Amount);
        }
    }
}
