using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data
{
    public class TableRepository
    {
        private efapiContext _context;

        public TableRepository(string connStr) {
            var options = new DbContextOptionsBuilder<efapiContext>();
            options.UseSqlServer(connStr);
            _context = new efapiContext(options.Options);
        }

        public async IAsyncEnumerable<Table> GetAll() {
            foreach (var t in await _context.Table.ToListAsync()) {
                yield return t;
            }
        }
    }
}
