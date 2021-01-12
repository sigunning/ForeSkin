using System;
using System.Collections.Generic;
using System.Text;

namespace ForeScore.LogOn
{
    /// <summary>
    /// Simple platform specific service that is responsible for locating a 
    /// </summary>
    public interface IParentWindowLocatorService
    {
       object GetCurrentParentWindow();
    }
}
