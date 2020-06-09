using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToStoreAction : MoveAction
{
    public StoreKnowledge knowledge;

    public MoveToStoreAction(Navigation navigation, StoreKnowledge knowledge) :
        base(navigation, knowledge.POSITION, Destination.Store)
    {
        this.knowledge = knowledge;
    }

}
