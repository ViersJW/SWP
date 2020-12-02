﻿using SWP.Domain.Infrastructure;
using SWP.Domain.Models.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SWP.DataBase.Managers
{
    public class LogManager : DataManagerBase, ILogManager
    {
        public LogManager(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<LogRecord> CreateLogRecord(LogRecord record)
        {
            if (_context.LogRecords.Where(x => x.UserId == "Automation").Count() > 10)
            {
                _context.LogRecords.RemoveRange(_context.LogRecords.Where(x => x.UserId == "Automation"));
            }

            _context.LogRecords.Add(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public Task<int> DeleteLogRecord(int id)
        {
            _context.LogRecords.Remove(_context.LogRecords.FirstOrDefault(x => x.Id == id));
            return _context.SaveChangesAsync();
        }

        public List<LogRecord> GetGetLogRecords() => _context.LogRecords.ToList();

        public List<LogRecord> GetGetLogRecords(Func<LogRecord, bool> predicate) => _context.LogRecords.Where(predicate).ToList();

        public LogRecord GetLogRecord(int id) => _context.LogRecords.FirstOrDefault(x => x.Id == id);

        public List<TResult> GetLogRecordsWithSpecificData<TResult>(Func<LogRecord, TResult> selector) => _context.LogRecords.Select(selector).ToList();
    }
}
