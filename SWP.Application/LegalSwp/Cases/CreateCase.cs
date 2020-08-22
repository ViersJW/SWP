﻿using SWP.Domain.Infrastructure;
using SWP.Domain.Models.SWPLegal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SWP.Application.LegalSwp.Cases
{
    [TransientService]
    public class CreateCase
    {
        private readonly ILegalSwpManager legalSwpManager;
        public CreateCase(ILegalSwpManager legalSwpManager) => this.legalSwpManager = legalSwpManager;

        public Task<int> Create(int customerId, string profile, Request request) => 
            legalSwpManager.CreateCase(customerId, profile, new Case
            { 
                Name = request.Name,
                Signature = string.IsNullOrEmpty(request.Signature) ? "Brak" : request.Signature,
                CaseType = string.IsNullOrEmpty(request.CaseType) ? "Standard" : request.CaseType,
                Description = string.IsNullOrEmpty(request.Description) ? "Brak" : request.Description,
                Active = true,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                UpdatedBy = request.UpdatedBy
            });

        public class Request
        {
            public string Name { get; set; }
            public string Signature { get; set; }
            public string CaseType { get; set; }
            public string Description { get; set; }
            public bool Active { get; set; }
            public string UpdatedBy { get; set; }
        }


    }
}
