using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IAction
{
    bool isActive { get; }
    bool isUsable { get; }

    void enable();
    void disable();
    void toggle();
}
