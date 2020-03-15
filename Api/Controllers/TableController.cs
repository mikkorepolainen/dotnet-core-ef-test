using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Data;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ILogger<TableController> _logger;
        private readonly TableRepository _data;

        public TableController(ILogger<TableController> logger, TableRepository data)
        {
            _logger = logger;
            _data = data;
        }

        [HttpGet]
        public IAsyncEnumerable<Table> Get()
        {
            return _data.GetAll();
            /*
            return Enumerable.Range(1, 5).Select(index => new Table
            {
                id = index,
                Value = index.ToString()
            })
            .ToArray();
            */
        }
    }
}
