using CloudX.Shared;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BaseX;

namespace VerifiedUsersOnly
{
    public class VerifiedUsersOnly : NeosMod
    {
        public override string Name => "VerifiedUsersOnly";
        public override string Author => "xLinka";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/xLinka/VerifiedUsersOnly";

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<bool> modActive = new ModConfigurationKey<bool>("Set Hosted Worlds to Verified Users Only", "This sets every world you are currently hosting to Verified Users Only", () => false);

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<string> verifyUrl = new ModConfigurationKey<string>("Verification URL", "The URL to use for verifying users", () => "https://verify.xlinka.com/verifyuser");

        private static ModConfiguration config;
        private static readonly HttpClient client = new HttpClient();

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            Harmony harmony = new Harmony("xLinka.VerfiedUsersOnly");
            harmony.PatchAll();
        }

        private static async Task<bool> IsUserVerified(string userId)
        {
            try
            {
                var response = await client.GetAsync($"{config.GetValue(verifyUrl)}?userId={userId}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return bool.Parse(result);
            }
            catch (Exception ex)
            {
                UniLog.Log($"Error verifying user {userId}: {ex.Message}");
                return false;
            }
        }

        [HarmonyPatch(typeof(World), "VerifyJoinRequest")]
        class ContactsChecker
        {
            static async Task<JoinGrant> Postfix(Task<JoinGrant> __result, World __instance, JoinRequestData joinRequest)
            {
                if (!config.GetValue(modActive))
                    return await __result;

                if (string.IsNullOrEmpty(joinRequest.userID))
                    return await __result;

                // Check if user is verified
                bool isVerified = await IsUserVerified(joinRequest.userID);
                if (!isVerified)
                    return JoinGrant.Deny("Only verified users are allowed to join\n(World is set to Verified Users Only)");

                return await __result;
            }
        }
    }
}
