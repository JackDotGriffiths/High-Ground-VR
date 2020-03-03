using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTesting : MonoBehaviour
{
    public UnitComponent m_testingUnit;
    private Unit enemyUnit;


    // Start is called before the first frame update
    void Start()
    {
        m_testingUnit.enemyUnitConstructor();
        enemyUnit = m_testingUnit.unit;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
