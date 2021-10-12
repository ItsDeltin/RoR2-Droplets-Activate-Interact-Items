using System;
using System.Collections.Generic;
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


        // Vanilla rebalancing settings
        /// <summary>The number of fireworks to fire when interacting with an item. Overrides the default</summary>
        public static EvaluatableConfigEntry InteractFireworkCount;


        // The mod default balancing.
        static DropletFormulaDefaults MonsterToothDefaults = new DropletFormulaDefaults { FireworkCount = "1 + n", FireworkDamageCoefficent = "2", SquidActivationChance = "0", SquidAttackSpeed = "n * 5", SquidHealth = "20", FireworkActivationChance = "100", DefiantGougeActivationChance = "0", DefiantGougeMonsterCredit = "n * 20" };
        static DropletFormulaDefaults BandolierDefaults = new DropletFormulaDefaults { FireworkCount = "1 + n * 3", FireworkDamageCoefficent = "2", SquidActivationChance = "0", SquidAttackSpeed = "n * 10", SquidHealth = "40", FireworkActivationChance = "100", DefiantGougeActivationChance = "0", DefiantGougeMonsterCredit = "n * 20" };
        static DropletFormulaDefaults GhorsTomeDefaults = new DropletFormulaDefaults { FireworkCount = "1 + n * 4", FireworkDamageCoefficent = "2", SquidActivationChance = "0", SquidAttackSpeed = "n * 15", SquidHealth = "60", FireworkActivationChance = "100", DefiantGougeActivationChance = "0", DefiantGougeMonsterCredit = "n * 20" };


        /// <summary>Initializes the config data.</summary>
        public static void Configure()
        {
            MonsterTooth = new InteractableEventSourceConfig("02 - Picked up Monster Tooth healing orb", "Monster Tooth", MonsterToothDefaults, true);
            Bandolier = new InteractableEventSourceConfig("03 - Picked up Bandolier ammo pack", "Bandolier", BandolierDefaults, false);
            GhorsTome = new InteractableEventSourceConfig("04 - Picked up Ghors Tome gold", "Ghors Tome", GhorsTomeDefaults, false);

            InteractFireworkCount = new EvaluatableConfigEntry(
                sectionName: "01 - Vanilla Rebalance",
                name: "Interact Firework Launch Count",
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

        protected string SourceItem { get; private set; }

        readonly EvaluatableConfigEntry _fireworkActivationChance;
        readonly EvaluatableConfigEntry _fireworkCount;
        readonly EvaluatableConfigEntry _fireworkDamageCoefficent;

        readonly EvaluatableConfigEntry _squidActivationChance;
        readonly EvaluatableConfigEntry _squidAttackSpeed;
        readonly EvaluatableConfigEntry _squidHealth;

        readonly EvaluatableConfigEntry _defiantGougeActivationChance;
        readonly EvaluatableConfigEntry _defiantGougeMonsterCredit;

        public InteractableEventSourceConfig(string sectionName, string sourceItem, DropletFormulaDefaults defaultValue, bool supportsActivationChance)
        {
            SectionName = sectionName;
            SourceItem = sourceItem;
            
            _fireworkActivationChance = Bind("Fireworks - Activation Chance", MakeDescription("The chance to activate fireworks", SourceItem), defaultValue.FireworkActivationChance);
            _fireworkCount = Bind("Fireworks - Launch Count", MakeDescription("The number of fireworks spawned", FireworkItemName), defaultValue.FireworkCount);
            _fireworkDamageCoefficent = Bind("Firework Damage Coefficent", MakeDescription("The damage coefficent of the spawned fireworks", FireworkItemName, "The vanilla default is 3"), defaultValue.FireworkDamageCoefficent);
            
            _squidActivationChance = Bind("Squid Polyp - Activation Chance", MakeDescription("The chance to spawn squids", SourceItem), defaultValue.SquidActivationChance);
            _squidAttackSpeed = Bind("Squid Polyp - Attack Speed", MakeDescription("The Squid Polyp attack speed", SquidItemName), defaultValue.SquidAttackSpeed);
            _squidHealth = Bind("Squid Polyp - Health", MakeDescription("The health of the Squid Polyp", SquidItemName), defaultValue.SquidHealth);
            
            _defiantGougeActivationChance = Bind("Defiant Gouge - Activation Chance", MakeDescription("The chance monsters will spawn", SourceItem), defaultValue.DefiantGougeActivationChance);
            _defiantGougeMonsterCredit = Bind("Defiant Gouge - Monster Credit", MakeDescription("The spawn credits used to spawn enemies", DefiantGougeName), defaultValue.DefiantGougeMonsterCredit);
        }

        EvaluatableConfigEntry Bind(string name, string description, string formula) =>
            new EvaluatableConfigEntry(SectionName, name, description, formula, parameters: "n");

        string MakeDescription(string description, string activeItem, string postDescription = null) =>
            description + " when picking up a " + SourceItem + ". " + (postDescription == null ? "" : postDescription + ". ") + StackParameterText(activeItem);


        ///<summary>Gets the chance to spawn fireworks</summary>
        public float GetFireworkActivationChance(int stackCount) => _fireworkActivationChance.Evaluate(stackCount);

        /// <summary>Gets the number of fireworks to be fired</summary>
        public int GetFireworkCount(int stackCount) => (int)_fireworkCount.Evaluate(stackCount);

        /// <summary>Gets the damage coefficient of fireworks</summary>
        public float GetFireworkDamageCoefficient(int stackCount) => _fireworkDamageCoefficent.Evaluate(stackCount);

        /// <summary>Gets the chance to spawn squids</summary>
        public float GetSquidActivationChance(int stackCount) => (int)_squidActivationChance.Evaluate(stackCount);

        /// <summary>Gets the attack speed of a squid polyp</summary>
        public int GetSquidAttackSpeed(int stackCount) => (int)_squidAttackSpeed.Evaluate(stackCount);

        /// <summary>Gets the health of a squid polyp</summary>
        public int GetSquidHealth(int stackCount) => (int)_squidHealth.Evaluate(stackCount);

        /// <summary>Gets the chance to spawn monsters</summary>
        public float GetDefiantGougeActivationChance(int stackCount) => _defiantGougeActivationChance.Evaluate(stackCount);

        /// <summary>Gets the monster credit</summary>
        public float GetDefiantGougeMonsterCredit(int stackCount) => _defiantGougeMonsterCredit.Evaluate(stackCount);
    }


    /// <summary>The default balance for a droplet</summary>
    struct DropletFormulaDefaults
    {
        public string FireworkActivationChance; // The chance to spawn fireworks.
        public string FireworkCount; // The number of fireworks fired.
        public string FireworkDamageCoefficent; // The damage of the fireworks.

        public string SquidActivationChance; // The chance to spawn a squid.
        public string SquidAttackSpeed; // The attack speed of the squids.
        public string SquidHealth; // The health of the squids.

        public string DefiantGougeActivationChance; // The chance to spawn monsters.
        public string DefiantGougeMonsterCredit; // The amount of monster credits that can be used to spawn enemies.
    }


    /// <summary>Math expression setting</summary>
    class EvaluatableConfigEntry
    {
        ///<summary>The current expression value</summary>
        public Expression Expression { get; private set; }

        readonly Expression _defaultExpression; // The default expression when the user has a bad input.
        readonly string[] _parameters; // The parameters in the formula.
        readonly string _sectionName;
        readonly string _name;

        public EvaluatableConfigEntry(string sectionName, string name, string description, string defaultFormula, params string[] parameters)
        {
            _defaultExpression = Expression.FromString(defaultFormula, parameters);
            _parameters = parameters;
            _sectionName = sectionName;
            _name = name;

            var entry = Droplets_Activate_Interact_Items.Configuration.Bind(sectionName, name, defaultFormula, description);

            // Set initial value
            Set(entry.Value);

            // Hot reload
            entry.SettingChanged += (obj, args) => Set(entry.Value);
        }

        void Set(string expression)
        {
            try
            {
                Expression = Expression.FromString(expression, _parameters);
            }
            catch (Math.Parse.SyntaxErrorException error)
            {
                Droplets_Activate_Interact_Items.Log.LogError(Title() + ": Failed to parse '" + expression + "': " + error.Message);
                Expression = _defaultExpression;
            }
            catch (Exception ex)
            {
                Droplets_Activate_Interact_Items.Log.LogError(Title() + ": Failed to parse '" + expression + "': " + ex);
                Expression = _defaultExpression;
            }
            Droplets_Activate_Interact_Items.Log.LogInfo(DebugName());
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

            return Expression.Evaluate(new EvaluateInfo(inputParameters));
        }

        string Title() => _sectionName + "/" + _name;
        string DebugName() => Title() + ": " + Expression.ToString();
    }
}