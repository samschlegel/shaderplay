using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SlimeSettings : ScriptableObject
{
    [Header("SimulationSettings")]
    public int width = 256;
    public int height = 256;
    public int numAgents = 100;
    public int stepsPerTick = 1;
    
    [Header("AgentSettings")]
    public float agentSpeed = 1;
    public float agentTurnRateDeg = 360;
    public float sensorAngleDeg = 30;
    public float sensorOffset = 30;
    public int sensorSize = 1;
    public float sameColorWeight = 1;
    public float differentColorWeight = -1;
    public float eatWeight = 0;

    [Header("Trail Settings")]
    public float trailWeight = 1;
    public float exponentialDecayRate = 1;
    public float linearDecayRate = 0;
    public float diffuseRate = 1;
}
