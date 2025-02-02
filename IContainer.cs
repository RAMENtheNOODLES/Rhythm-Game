using System;
using System.Collections.Generic;

namespace RhythmGame;

public interface IContainer
{
    int Size { get; init; }
    List<IInteractable> Items { get; }

    public bool AddItem(IInteractable item)
    {
        if (Items.Count + 1 > Size)
            return false;
        Items.Add(item);
        return true;
    }

    public IInteractable RemoveItem(int slot)
    {
        if (slot > Items.Count)
            throw new IndexOutOfRangeException("Slot " + slot + " does not exist...");
        var output = Items[slot];
        Items.RemoveAt(slot);
        return output;
    }
}