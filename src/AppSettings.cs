using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace benzconfig;

public class AppSettings
{
    public SeasonSettings Summer { get; set; } = new();
    public SeasonSettings Winter { get; set; } = new();
}

public class SeasonSettings
{
    public double PropCity { get; set; }
    public double PropHighway { get; set; }
    public double RateCity { get; set; }
    public double RateHighway { get; set; }
}