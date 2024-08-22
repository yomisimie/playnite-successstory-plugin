﻿using Playnite.SDK.Models;
using CommonPluginsShared;
using SuccessStory.Models;
using System;
using System.Linq;
using static CommonPluginsShared.PlayniteTools;
using CommonPluginsStores.Gog;
using System.Collections.ObjectModel;
using CommonPluginsStores.Models;
using System.Collections.Generic;
using Playnite.SDK;

namespace SuccessStory.Clients
{
    public class GogAchievements : GenericAchievements
    {
        protected static readonly Lazy<GogApi> gogAPI = new Lazy<GogApi>(() => new GogApi(PluginDatabase.PluginName));
        internal static GogApi GogAPI => gogAPI.Value;


        public GogAchievements() : base("GOG", CodeLang.GetGogLang(API.Instance.ApplicationSettings.Language))
        {
            GogAPI.SetLanguage(API.Instance.ApplicationSettings.Language);
        }


        public override GameAchievements GetAchievements(Game game)
        {
            GameAchievements gameAchievements = SuccessStory.PluginDatabase.GetDefault(game);
            List<Achievements> AllAchievements = new List<Achievements>();

            if (IsConnected())
            {
                try
                {
                    ObservableCollection<GameAchievement> gogAchievements = GogAPI.GetAchievements(game.GameId, GogAPI.CurrentAccountInfos);    
                    if (gogAchievements?.Count > 0)
                    {
                        AllAchievements = gogAchievements.Select(x => new Achievements 
                        {
                            ApiName = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            UrlUnlocked = x.UrlUnlocked,
                            UrlLocked = x.UrlLocked,
                            DateUnlocked = x.DateUnlocked,
                            Percent = x.Percent,
                            GamerScore = x.GamerScore
                        }).ToList();
                        gameAchievements.Items = AllAchievements;
                    }
                    else
                    {
                        if (!GogAPI.IsUserLoggedIn)
                        {
                            ShowNotificationPluginNoAuthenticate(ResourceProvider.GetString("LOCSuccessStoryNotificationsGogNoAuthenticate"), ExternalPlugin.GogLibrary);
                        }
                    }
                    
                    // Set source link
                    if (gameAchievements.HasAchievements)
                    {
                        gameAchievements.SourcesLink = GogAPI.GetAchievementsSourceLink(game.Name, game.GameId, GogAPI.CurrentAccountInfos);
                    }
                }
                catch (Exception ex)
                {
                    ShowNotificationPluginError(ex);
                    return gameAchievements;
                }
            }
            else
            {
                ShowNotificationPluginNoAuthenticate(ResourceProvider.GetString("LOCSuccessStoryNotificationsGogNoAuthenticate"), ExternalPlugin.GogLibrary);
            }

            gameAchievements.SetRaretyIndicator();
            return gameAchievements;
        }


        #region Configuration
        public override bool ValidateConfiguration()
        {
            if (PlayniteTools.IsDisabledPlaynitePlugins("GogLibrary"))
            {
                ShowNotificationPluginDisable(ResourceProvider.GetString("LOCSuccessStoryNotificationsGogDisabled"));
                return false;
            }
            else
            {
                if (CachedConfigurationValidationResult == null)
                {
                    CachedConfigurationValidationResult = IsConnected();

                    if (!(bool)CachedConfigurationValidationResult)
                    {
                        ShowNotificationPluginNoAuthenticate(ResourceProvider.GetString("LOCSuccessStoryNotificationsGogNoAuthenticate"), ExternalPlugin.GogLibrary);
                    }
                    else
                    {
                        CachedConfigurationValidationResult = IsConfigured();

                        if (!(bool)CachedConfigurationValidationResult)
                        {
                            ShowNotificationPluginNoConfiguration(ResourceProvider.GetString("LOCSuccessStoryNotificationsGogBadConfig"));
                        }
                    }
                }
                else if (!(bool)CachedConfigurationValidationResult)
                {
                    ShowNotificationPluginErrorMessage();
                }

                return (bool)CachedConfigurationValidationResult;
            }
        }

        public override bool IsConnected()
        {
            if (CachedIsConnectedResult == null)
            {
                CachedIsConnectedResult = GogAPI.IsUserLoggedIn;
            }

            return (bool)CachedIsConnectedResult;
        }

        public override bool IsConfigured()
        {
            return IsConnected();
        }

        public override bool EnabledInSettings()
        {
            return PluginDatabase.PluginSettings.Settings.EnableGog;
        }

        public override void ResetCachedConfigurationValidationResult()
        {
            CachedConfigurationValidationResult = null;
            GogAPI.ResetIsUserLoggedIn();
        }

        public override void ResetCachedIsConnectedResult()
        {
            CachedIsConnectedResult = null;
            GogAPI.ResetIsUserLoggedIn();
        }
        #endregion
    }
}
