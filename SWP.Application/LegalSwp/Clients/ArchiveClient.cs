﻿using SWP.Domain.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SWP.Application.LegalSwp.Clients
{
    [TransientService]
    public class ArchiveClient
    {
        private readonly ILegalSwpManager legalSwpManager;
        public ArchiveClient(ILegalSwpManager legalSwpManager) => this.legalSwpManager = legalSwpManager;

        public int CountAllArchivedClients() => legalSwpManager.CountArchivedClients();

        public Task<int> ArchivizeClient(int clientId, string user) => legalSwpManager.ArchivizeClient(clientId, user);
        public Task<int> RecoverClient(int clientId, string user) => legalSwpManager.RecoverClient(clientId, user);
    }
}
