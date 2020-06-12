using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToStoreAction : MoveAction
{
    private StoreKnowledge knowledge;

    public MoveToStoreAction(Navigation navigation, StoreKnowledge knowledge) :
        base(navigation, knowledge.LOCATION, Destination.Store)
    {
        this.knowledge = knowledge;
    }

    public StoreKnowledge Knowledge
    {
        get => knowledge;
    }

}
