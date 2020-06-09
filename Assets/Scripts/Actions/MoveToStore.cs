using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToStore : IAction
{
    public Human human;
    public StoreKnowledge knowledge;

    public MoveToStore(Human human, StoreKnowledge knowledge)
    {
        this.human = human;
        this.knowledge = knowledge;
    }

    public void Execute()
    {
        Navigation navigation = human.navigation;
        navigation.MoveTo(knowledge.POSITION);
    }

}
