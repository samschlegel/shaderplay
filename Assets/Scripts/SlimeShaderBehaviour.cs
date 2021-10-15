using System;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using ComputeShaderUtility;
using Random = UnityEngine.Random;

public struct Agent
{
    public float2 Position;
    public float Heading;
    public Color Color;
}

public class SlimeShaderBehaviour: MonoBehaviour
{
    public ComputeShader shader;

    private ComputeBuffer m_Agents;

    public RenderTexture trailMap;
    public RenderTexture diffusedTrailMap;

    public SlimeSettings settings;
    
    // Start is called before the first frame update
    private void Start()
    {
        Init();
    }

    private void Init()
    {
        ComputeHelper.CreateRenderTexture(ref trailMap, settings.width, settings.height);
        ComputeHelper.CreateRenderTexture(ref diffusedTrailMap, settings.width, settings.height);
        
        shader.SetTexture(0, "trail", trailMap);
        shader.SetTexture(1, "trail", trailMap);
        shader.SetTexture(0, "diffused_trail", diffusedTrailMap);
        shader.SetTexture(1, "diffused_trail", diffusedTrailMap);
        
        var data = new Agent[settings.numAgents];
        var radius = Mathf.Min(settings.width, settings.height) / 4;
        for (int i = 0; i < data.Length; i++)
        {
            var p = Random.insideUnitCircle;
            var r = Random.value;
            Color color;
            if (r < 0.33333f)
            {
                color = Color.red;
            }
            else if (r < 0.6666666f)
            {
                color = Color.green;
            }
            else
            {
                color = Color.blue;
            }

            color = Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1);
            color = color.linear;
            color /= Mathf.Sqrt(Mathf.Pow(color.r, 2) + Mathf.Pow(color.g, 2) + Mathf.Pow(color.b, 2));
            color.a = 1;

            data[i] = new Agent()
            {
                // Position = new float2(Random.Range(0, settings.width), Random.Range(0, settings.height)),
                Position = new float2(settings.width / 2f, settings.height / 2f) + new float2(p * radius),
                Heading = Random.value * Mathf.PI * 2 - Mathf.PI,
                Color = color,
            };
        }

        ComputeHelper.CreateAndSetBuffer<Agent>(ref m_Agents, data, shader, "agents", 0);
        shader.SetInt("num_agents", settings.numAgents);
    }

    private void FixedUpdate()
    {
        shader.SetFloat("time", Time.time);
        shader.SetFloat("delta_time", Time.fixedDeltaTime);
        shader.SetFloats("resolution", settings.width, settings.height);
        
        shader.SetFloat("agent_speed", settings.agentSpeed);
        shader.SetFloat("agent_turn_rate_rad", settings.agentTurnRateDeg * Mathf.PI / 180);
        shader.SetFloat("sensor_angle_rad", settings.sensorAngleDeg * Mathf.PI / 180);
        shader.SetFloat("sensor_offset", settings.sensorOffset);
        shader.SetInt("sensor_size", settings.sensorSize);
        shader.SetFloat("same_color_weight", settings.sameColorWeight);
        shader.SetFloat("different_color_weight", settings.differentColorWeight);
        
        shader.SetFloat("eat_weight", settings.eatWeight);
        shader.SetFloat("trail_weight", settings.trailWeight);
        shader.SetFloat("diffuse_rate", settings.diffuseRate);
        shader.SetFloat("exponential_decay_rate", settings.exponentialDecayRate);
        shader.SetFloat("linear_decay_rate", settings.linearDecayRate);
        
        foreach (var i in Enumerable.Range(0, settings.stepsPerTick))
        {
            ComputeHelper.Dispatch(shader, settings.numAgents, kernelIndex: 0);
            ComputeHelper.Dispatch(shader, settings.width, settings.height, kernelIndex: 1);
            ComputeHelper.CopyRenderTexture(diffusedTrailMap, trailMap);
        }
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        
        Graphics.Blit(trailMap, dest);
    }

    private void OnDestroy()
    {
        ComputeHelper.Release(m_Agents);
    }
}