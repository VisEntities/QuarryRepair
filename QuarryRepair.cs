using Newtonsoft.Json;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Rewritten from scratch and maintained to present by VisEntities
 * Originally created by Orange, up to version 1.0.21
 */

namespace Oxide.Plugins
{
    [Info("Quarry Repair", "VisEntities", "2.0.0")]
    [Description("Brings back the ability to repair deployable quarries and pump jacks.")]
    public class QuarryRepair : RustPlugin
    {
        #region Fields

        private static QuarryRepair _plugin;
        private static Configuration _config;

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Health Per Hit")]
            public float HealthPerHit { get; set; }

            [JsonProperty("Repair Cost")]
            public List<ItemInfo> RepairCost { get; set; }
        }

        public class ItemInfo
        {
            [JsonProperty("Shortname")]
            public string Shortname { get; set; }

            [JsonProperty("Amount")]
            public int Amount { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                HealthPerHit = 25f,
                RepairCost = new List<ItemInfo>
                {
                    new ItemInfo
                    {
                        Shortname = "wood",
                        Amount = 100
                    },
                    new ItemInfo
                    {
                        Shortname = "metal.fragments",
                        Amount = 100
                    },
                    new ItemInfo
                    {
                        Shortname = "metal.refined",
                        Amount = 5
                    },
                }
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        private void OnHammerHit(BasePlayer player, HitInfo hitInfo)
        {
            if (player == null || hitInfo == null)
                return;

            BaseResourceExtractor resourceExtractor = hitInfo.HitEntity as BaseResourceExtractor;
            if (resourceExtractor == null)
                return;

            if (resourceExtractor.SecondsSinceAttacked <= 30f)
                return;

            float damage = resourceExtractor.MaxHealth() - resourceExtractor.Health();
            if (damage <= 0)
                return;

            List<ItemAmount> repairCost = new List<ItemAmount>();
            foreach (ItemInfo item in _config.RepairCost)
            {
                ItemDefinition itemDef = ItemManager.FindItemDefinition(item.Shortname);
                if (itemDef != null)
                    repairCost.Add(new ItemAmount(itemDef, item.Amount));
            }

            foreach (ItemAmount item in repairCost)
            {
                int playerAmount = player.inventory.GetAmount(item.itemid);
                if (playerAmount < item.amount)
                {
                    resourceExtractor.OnRepairFailedResources(player, repairCost);
                    return;
                }
            }

            float healthToAdd = Math.Min(_config.HealthPerHit, damage);

            foreach (ItemAmount item in repairCost)
            {
                int amountToDeduct = (int)item.amount;
                int amountTaken = player.inventory.Take(null, item.itemid, amountToDeduct);

                if (amountTaken > 0)
                    player.Command("note.inv", item.itemid, -amountTaken);
            }

            resourceExtractor.health += healthToAdd;
            resourceExtractor.SendNetworkUpdate(BasePlayer.NetworkQueue.Update);

            if (resourceExtractor.Health() >= resourceExtractor.MaxHealth())
                resourceExtractor.OnRepairFinished();
            else
                resourceExtractor.OnRepair();
        }

        #endregion Oxide Hooks
    }
}