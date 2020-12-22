using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace NearFutureUtils
{
  public class ModuleAdvancedColorChanger : PartModule, IScalarModule
  {
    [KSPField]
    public float colorScale = 1f;

    [KSPField]
    public string moduleID;

    [KSPField]
    public float animRate;

    [KSPField]
    public string shaderProperty;

    [KSPField]
    public FloatCurve redCurve = new FloatCurve();

    [KSPField]
    public FloatCurve greenCurve = new FloatCurve();

    [KSPField]
    public FloatCurve blueCurve = new FloatCurve();

    [KSPField]
    public FloatCurve alphaCurve = new FloatCurve();

    [KSPField]
    public string includedTransformList;

    [KSPField(isPersistant = true)]
    public bool animState = false;


    [KSPField]
    public bool toggleInEditor = true;

    [KSPField]
    public bool toggleInFlight = true;

    [KSPField]
    public string eventOnName = string.Empty;

    [KSPField]
    public string eventOffName = string.Empty;

    [KSPField]
    public KSPActionGroup defaultActionGroup;

    [KSPEvent(unfocusedRange = 5f, guiActiveUnfocused = true, guiActive = true, guiActiveEditor = true, guiName = "#autoLOC_6001829")]
    public virtual void ToggleEvent()
    {
      animState = !animState;

      SetState(animState, false);
      SetEventName(animState);
    }

    [KSPAction("Toggle Color", KSPActionGroup.REPLACEWITHDEFAULT)]
    public virtual void ToggleAction(KSPActionParam param)
    {
      ToggleEvent();
    }

    public EventData<float, float> OnMoving => OnMove;

    public EventData<float> OnStop => OnStopped;
    private EventData<float, float> OnMove = new EventData<float, float>("OnMove");
    private EventData<float> OnStopped = new EventData<float>("OnStop");
    public string ScalarModuleID
    {
      get { return moduleID; }
    }
    public bool CanMove
    {
      get { return true; }
    }
    public float GetScalar
    {
      get { return animationFraction; }
    }


    public void SetScalar(float t)
    {
      animationGoal = t;
    }

    public bool IsMoving()
    {
      return true;
    }



    public void SetUIWrite(bool value)
    { }
    public void SetUIRead(bool value)
    { }

    protected float animationFraction = 0f;
    protected float animationGoal = 0f;
    protected List<Renderer> targetRenderers;
    public override void OnAwake()
    {
      base.OnAwake();
      BaseAction baseAction = base.Actions["ToggleAction"];
      if (baseAction.actionGroup == KSPActionGroup.REPLACEWITHDEFAULT)
      {
        baseAction.actionGroup = defaultActionGroup;
      }
      if (baseAction.defaultActionGroup == KSPActionGroup.REPLACEWITHDEFAULT)
      {
        baseAction.defaultActionGroup = defaultActionGroup;
      }
    }
    protected void Start()
    {

      SetupRenderers();
      SetState(animState, true);
      SetEventName(animState);

      Utils.Log($"[ModuleAdvancedColorChanger] {moduleID} Set up {targetRenderers.Count} renderers");
    }

    protected void SetState(bool newState, bool snapTo)
    {
      if (targetRenderers != null)
      {
        if (newState)
        {
          animationGoal = 1f;
          if (snapTo)
            animationFraction = 1f;
        }
        else
        {
          animationGoal = 0f;
          if (snapTo)
            animationFraction = 0f;
        }

        Color c = new Color(redCurve.Evaluate(animationFraction) * colorScale, greenCurve.Evaluate(animationFraction) * colorScale, blueCurve.Evaluate(animationFraction) * colorScale, alphaCurve.Evaluate(animationFraction) * colorScale);

        foreach (Renderer r in targetRenderers)
        {
          r.material.SetColor(shaderProperty, c);
        }
      }

    }

    protected void SetEventName(bool newState)
    {
      if (newState)
        Events["ToggleEvent"].guiName = Localizer.Format(eventOffName);
      else
        Events["ToggleEvent"].guiName = Localizer.Format(eventOnName);
    }

    protected void SetupRenderers()
    {
      targetRenderers = new List<Renderer>();
      if (includedTransformList == "")
      {
        foreach (Transform x in part.GetComponentsInChildren<Transform>())
        {
          Renderer r = x.GetComponent<Renderer>();

          if (r != null && r.material.HasProperty(shaderProperty)) targetRenderers.Add(r);
        }
      }
      else
      {
        string[] allXformNames = includedTransformList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string xformName in allXformNames)
        {
          Transform[] xforms = part.FindModelTransforms(xformName);
          foreach (Transform x in xforms)
          {
            Renderer r = x.GetComponent<Renderer>();

            if (r != null && r.material.HasProperty(shaderProperty))
              targetRenderers.Add(r);
          }
        }
      }
    }
    protected void Update()
    {
      if (targetRenderers != null && (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight))
      {
        animationFraction = Mathf.MoveTowards(animationFraction, animationGoal, TimeWarp.deltaTime * animRate);

        Color c = new Color(redCurve.Evaluate(animationFraction) * colorScale, greenCurve.Evaluate(animationFraction) * colorScale, blueCurve.Evaluate(animationFraction) * colorScale, alphaCurve.Evaluate(animationFraction) * colorScale);

        foreach (Renderer r in targetRenderers)
        {
          r.material.SetColor(shaderProperty, c);
        }
        if (animationFraction == animationGoal)
        {
          if (animationGoal == 0)
            animState = false;
          else
            animState = true;

          SetEventName(animState);
        }
      }
    }

  }
}
