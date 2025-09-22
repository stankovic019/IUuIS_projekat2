using NetworkService.Model;
using System.Collections.Generic;

namespace NetworkService.DTOs
{
    public class ValveGroupDto
    {
        public ValveType Type { get; set; }
        public string Header;
        public List<Valve> Valves { get; set; }

        public ValveGroupDto(ValveType type, string header, List<Valve> valves)
        {
            Type = type;
            Header = header;
            Valves = valves;
        }
    }
}
