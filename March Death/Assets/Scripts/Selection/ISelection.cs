using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public interface ISelection
{
    int Count { get; }
    void Select(List<Selectable> selectables);
    void Select(Selectable selectable);
    void Deselect(Selectable selectable);
    void Clear();
    bool Contains(Selectable selectable);
    bool IsTroop();
    void Remove(Selectable selectable);
    ArrayList ToArrayList();
    Selectable[] ToArray();
    List<Selectable> ToList();

}
