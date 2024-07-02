using Arcatech;
using Arcatech.Items;
using Arcatech.Skills;
using Arcatech.Texts;
using UnityEditor;
using UnityEngine;

public class CreateDescriptionsForItems : MonoBehaviour
{
    [MenuItem("Tools/Arcatech/Create and assign descriptions for weapon and skill configs")]
    private static void FindScriptableObjects()
    {
        var sk = Resources.FindObjectsOfTypeAll<SerializedSkillConfiguration>();
        var items = Resources.FindObjectsOfTypeAll<ItemSO>();
        string prefix = "desc_";
        string sk_prefix = "skill_";
        string i_prefix = "item_";

        foreach (var s in sk)
        {
            string descriptionFileTitle = prefix+sk_prefix+s.name+".asset";
            if (s.Description == null)
            {
                //if (!s.name.Contains(sk_prefix))
                //{
                //    s.name = sk_prefix + s.name;
                //}                
                ExtendedText asset = ScriptableObject.CreateInstance<ExtendedText>();
                string name = descriptionFileTitle;
                AssetDatabase.CreateAsset(asset, Constants.Texts.c_SkillsDesc+name);

                AssetDatabase.SaveAssets();
                s.Description = asset;
            }
        }

        foreach (var s in items)
        {
            string descriptionFileTitle = prefix + i_prefix + s.name + ".asset";
            if (s.Description == null)
            {
                //if (!s.name.Contains(i_prefix))
                //{
                //    s.name = i_prefix + s.name;
                //}
                ExtendedText asset = ScriptableObject.CreateInstance<ExtendedText>();
                string name = descriptionFileTitle;
                AssetDatabase.CreateAsset(asset, Constants.Texts.c_WeaponsDesc + name);

                AssetDatabase.SaveAssets();
                s.Description = asset;
            }
        }
    }





}
