using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using BepInEx.Configuration;
using Deltin.Math;
using static Deltin.Constants;

namespace Deltin
{
    static class DropletConfig
    {
        /// <summary>Monster tooth balance</summary>
        public static InteractableEventSourceConfig MonsterTooth;
        /// <summary>Bandolier balance</summary>
        public static InteractableEventSourceConfig Bandolier;
        /// <summary>Ghor's Tome balance</summary>
        public static InteractableEventSourceConfig GhorsTome;
        /// <summary>The number of fireworks to fire when interacting with an item. Overrides the default</summary>
        public static EvaluatableConfigEntry InteractFireworkCount;


        // The mod default balancing.
        static DropletFormulaDefaults MonsterToothDefaults = new DropletFormulaDefaults { FireworkCount = "1 + n", SquidAttackSpeed = "5 * n", SquidHealth = "20" };
        static DropletFormulaDefaults BandolierDefaults = new DropletFormulaDefaults { FireworkCount = "1 + n * 3", SquidAttackSpeed = "10 * n", SquidHealth = "40" };
        static DropletFormulaDefaults GhorsTomeDefaults = new DropletFormulaDefaults { FireworkCount = "1 + n * 4", SquidAttackSpeed = "15 * n", SquidHealth = "60" };


        /// <summary>Initializes the config data.</summary>
        public static void Configure(ManualLogSource logger, ConfigFile config)
        {
            MonsterTooth = new InteractableEventSourceConfig(logger, config, "Pick_Up_Monster_Tooth", "Monster Tooth", MonsterToothDefaults);
            Bandolier = new InteractableEventSourceConfig(logger, config, "Pick_Up_Bandolier", "Bandolier", BandolierDefaults);
            GhorsTome = new InteractableEventSourceConfig(logger, config, "Pick_Up_Ghors_Tome", "Ghors Tome", GhorsTomeDefaults);

            InteractFireworkCount = new EvaluatableConfigEntry(
                logger,
                config,
                sectionName: "VanillaRebalance",
                name: "FireworkLaunchCount",
                description: @"The number of fireworks that are fired when interacting. Since the buff may make fireworks a bit overtuned,
 this mod reduces the default. The vanilla formula is '4 + n * 4'. " + StackParameterText(FireworkItemName),
                defaultFormula: "2 + n * 2",
                "n");
        }
    }

    /// <summary>Interactable balancing from a certain droplet source</summary>
    class InteractableEventSourceConfig
    {
        public string SectionName { get; }

        readonly EvaluatableConfigEntry _fireworkCount;
        readonly EvaluatableConfigEntry _squidAttackSpeed;
        readonly EvaluatableConfigEntry _squidHealth;

        public InteractableEventSourceConfig(ManualLogSource logger, ConfigFile config, string sectionName, string sourceItem, DropletFormulaDefaults defaultValue)
        {
            SectionName = sectionName;
            
            _fireworkCount = Bind(logger, config, "FireworkLaunchCount", "The number of fireworks spawned", sourceItem, FireworkItemName, defaultValue.FireworkCount);
            _squidAttackSpeed = Bind(logger, config, "SquidAttackSpeed", "The Squid Polyp attack speed", sourceItem, SquidItemName, defaultValue.SquidAttackSpeed);
            _squidHealth = Bind(logger, config, "SquidHealth", "The health of the Squid Polyp", sourceItem, SquidItemName, defaultValue.SquidHealth);
        }

        EvaluatableConfigEntry Bind(ManualLogSource logger, ConfigFile config, string name, string description, string sourceItem, string item, string formula) =>
            new EvaluatableConfigEntry(
                logger,
                config,
                SectionName,
                name,
                description + " when picking up a " + sourceItem + ". " + StackParameterText(item),
                formula,
                parameters: "n");


        /// <summary>Gets the number of fireworks to be fired</summary>
        public int GetFireworkCount(int stackCount) => (int)_fireworkCount.Evaluate(stackCount);

        /// <summary>Gets the attack speed of a squid polyp</summary>
        public int GetSquidAttackSpeed(int stackCount) => (int)_squidAttackSpeed.Evaluate(stackCount);

        /// <summary>Gets the health of a squid polyp</summary>
        public int GetSquidHealth(int stackCount) => (int)_squidHealth.Evaluate(stackCount);
    }


    /// <summary>The default balance for a droplet</summary>
    struct DropletFormulaDefaults
    {
        public string FireworkCount;
        public string SquidAttackSpeed;
        public string SquidHealth;
    }


    /// <summary>Math expression setting</summary>
    class EvaluatableConfigEntry
    {
        ///<summary>The current expression value</summary>
        public Expression Expression { get; private set; }

        readonly Expression _defaultExpression; // The default expression when the user has a bad input.
        readonly string[] _parameters; // The parameters in the formula.
        readonly ManualLogSource _logger; // Log bad settings.
        readonly string _sectionName;
        readonly string _name;

        public EvaluatableConfigEntry(ManualLogSource logger, ConfigFile config, string sectionName, string name, string description, string defaultFormula, params string[] parameters)
        {
            _defaultExpression = defaultFormula;
            _parameters = parameters;
            _logger = logger;
            _sectionName = sectionName;
            _name = name;

            var entry = config.Bind(sectionName, name, defaultFormula, description);
            Set(entry.Value);
        }

        void Set(string expression)
        {
            try
            {
                Expression = expression;
                _logger.LogInfo(_sectionName + "/" + _name + ": " + Expression.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                Expression = _defaultExpression;
            }
        }

        /// <summary>Gets the formula's value</summary>
        public float Evaluate(params float[] values)
        {
            // values null check
            if (values == null)
                throw new NullReferenceException(nameof(values));

            // Make sure the number of provided values is equal to the number of parameters.
            if (values.Length != _parameters.Length)
                throw new ArgumentException("\"" + Expression.ToString() + "\" has " + _parameters.Length + " parameters, but " + values.Length + " values were provided");

            // Substitute parameters.
            Dictionary<string, float> inputParameters = new Dictionary<string, float>();
            for (int i = 0; i < values.Length; i++)
                inputParameters.Add(_parameters[i], values[i]);

            var evaluateInfo = new EvaluateInfo(inputParameters);

            try
            {
                return Expression.Evaluate(evaluateInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return _defaultExpression.Evaluate(evaluateInfo);
            }
        }
    }
}