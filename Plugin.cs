using BepInEx;
using System;
using UnityEngine;
using Utilla;
using UnityEngine.XR;
using System.Reflection;

namespace MonkeyPack
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private static XRNode rNode = XRNode.RightHand;
        private static bool GripPress;
        Vector3 controllerVel;
        Vector3 prevControllerPosition;
        Vector3 Scale = new Vector3(0.035f, 0.035f, 0.035f);
        Vector3 BackPos = new Vector3(0, 0.3042f, -0.32f);
        Quaternion BackRot = Quaternion.Euler(0, 180f, 0.7929f);
        GameObject playerHand;
        GameObject player;
        GameObject ukkiki;
        Transform playerHandFollower;
        Animator ukkikiAnim;
        Rigidbody ukkikiRb;
        float monkeyYRot;
        float timer;
        float lerpSpeed;
        float monekyDist;
        bool held;
        bool MonkeyReset;
        bool goingHome;
        bool home;

        void Start()
        {

            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            var str = Assembly.GetExecutingAssembly().GetManifestResourceStream("MonkeyPack.Assets.ukkiki");
            if (str == null)
                return;

            AssetBundle bundle = AssetBundle.LoadFromStream(str);
            if (bundle == null)
                return;

            var asset = bundle.LoadAsset<GameObject>("Ukkiki");
            asset = Instantiate(asset);
            ukkiki = asset;
            ukkikiAnim = ukkiki.GetComponentInChildren<Animator>();
            player = GameObject.Find("Global/Local VRRig/Local Gorilla Player/rig/body/").gameObject;
            ukkiki.transform.SetParent(player.transform);
            ukkiki.transform.localScale = Scale;
            ukkiki.transform.localPosition = BackPos;
            ukkiki.transform.localRotation = BackRot;
            playerHand = GameObject.Find("Global/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/");
            ukkiki.layer = 8;
            ukkikiAnim.Play("Hold");
            playerHandFollower = GorillaLocomotion.Player.Instance.leftHandFollower.transform;
        }

        void LateUpdate()
        {
            controllerVel = (playerHand.transform.position - prevControllerPosition) / Time.deltaTime;
            prevControllerPosition = playerHand.transform.position;
        }

        void FixedUpdate()
        {
            lerpSpeed = (monekyDist / 40f);
            timer += Time.deltaTime;
            monekyDist = Vector3.Distance(ukkiki.transform.position, player.transform.position);
            
            InputDevice rightController = InputDevices.GetDeviceAtXRNode(rNode);

            rightController.TryGetFeatureValue(CommonUsages.gripButton, out GripPress);
            if (GripPress)
            {
                Collider[] colliderArray = Physics.OverlapSphere(playerHand.transform.position, 0.23f);
                foreach(Collider collider in colliderArray)
                {
                    if (collider == ukkiki.GetComponent<Collider>())
                    {
                        ukkiki.transform.SetParent(playerHand.transform);
                        held = true;
                        ukkikiAnim.Play("Hold");
                        if (ukkikiRb != null)
                        {
                            ukkikiRb.isKinematic = true;
                        }
                        ukkiki.transform.localPosition = new Vector3(0.18f, 0, 0f);
                        ukkiki.transform.localRotation = Quaternion.Euler(0, 90, 90);
                        MonkeyReset = true;
                        
                    }
                }

                if (!held)
                {
                    if (ukkikiRb != null)
                    {
                        ukkikiRb.isKinematic = true;
                    }
                    ukkiki.transform.position = Vector3.Lerp(ukkiki.transform.position, player.transform.position, Time.deltaTime / lerpSpeed);
                    goingHome = true;
                    if (monekyDist <= 1)
                    {
                        ukkiki.transform.SetParent(player.transform);
                        ukkiki.transform.localPosition = BackPos;
                        ukkiki.transform.localRotation = BackRot;
                        ukkikiAnim.Play("Hold");
                        
                        goingHome = false;
                        home = true;
                    }
                    
                }

            }
            
            if (!GripPress)
            {
                if (goingHome)
                {
                    ukkikiRb.isKinematic = false;
                    goingHome = false;
                }
                if (held)
                {
                    home = false;
                    if (ukkiki.TryGetComponent(out Rigidbody rb))
                    {
                        ukkikiRb = rb;
                        rb.isKinematic = false;
                        ukkikiRb.velocity = controllerVel * 2;
                        held = false;
                        rb.constraints = RigidbodyConstraints.None;
                        ukkikiAnim.Play("Throw");
                    }
                    else
                    {
                        ukkiki.AddComponent<Rigidbody>();
                    }
                    ukkiki.transform.SetParent(null);
                }
                if (!held)
                {
                    if (ukkikiRb.velocity == new Vector3(0, 0, 0))
                    {
                        
                        
                        
                        ukkikiAnim.Play("JumpExited");
                        float upPos = ukkiki.transform.localPosition.y + 0.06f;
                        ukkikiRb.constraints = RigidbodyConstraints.FreezePositionX;
                        ukkikiRb.constraints = RigidbodyConstraints.FreezePositionZ;
                        if (MonkeyReset)
                        {
                            Physics.Raycast(ukkiki.transform.position, ukkiki.transform.TransformDirection(Vector3.down), out RaycastHit hit, 0.25f);
                            Quaternion rotPP = Quaternion.FromToRotation(ukkiki.transform.up, hit.normal);
                            ukkiki.transform.localPosition = new Vector3(ukkiki.transform.localPosition.x, upPos, ukkiki.transform.localPosition.z);
                            ukkiki.transform.localRotation = rotPP;
                            ukkikiRb.constraints = RigidbodyConstraints.FreezeRotationX;
                            ukkikiRb.constraints = RigidbodyConstraints.FreezeRotationZ;
                            MonkeyReset = false;
                        }
                    }
                }
                
            }
        }


    }
}
