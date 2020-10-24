﻿using SWP.Domain.Infrastructure.LegalApp;
using System.Threading.Tasks;

namespace SWP.Application.LegalSwp.Clients
{
    [TransientService]
    public class DeleteClient
    {
        private readonly ILegalSwpManager legalSwpManager;
        public DeleteClient(ILegalSwpManager legalSwpManager) => this.legalSwpManager = legalSwpManager;

        public Task<int> Delete(int id) => legalSwpManager.DeleteClient(id);
        public Task<int> Delete(string profile) => legalSwpManager.DeleteProfileClients(profile);
    }
}
