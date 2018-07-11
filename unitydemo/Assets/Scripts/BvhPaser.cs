using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace opdemo
{
    public class BvhPaser
    {
        // This indexMap is used to map the BVH joint to OP joint ID
        // for OP bvh data
        private static int[] indexMap = { 0, 0, 1, 4, 7, -1, 0, 2, 5, 8, -1, 0, 3, 6, 9, 12, -1, 9, 13, 16, 18, -1, 20, 22, 23, 24, -1, 20, 26, 27, 28, -1, 20, 30, 31, 32, -1, 20, 34, 35, 36, -1, 20, 38, 39, 40, -1, 9, 14, 17, 19, -1, 21, 42, 43, 44, -1, 21, 46, 47, 48, -1, 21, 50, 51, 52, -1, 21, 54, 55, 56, -1, 21, 58, 59, 60, -1 };
        // for CMU mocap data
        //public static int[] indexMap = { 0, -1, 0, 1, 4, 7, -1, -1, 0, 2, 5, 8, -1, 0, 3, 6, 9, 12, 15, -1, 9, 13, 16, 18, 20, 26, -1, 22, -1, 9, 14, 17, 19, 21, 46, -1, 42, -1, };

        public static AnimDataSet BvhToDataSet(string data)
        {
            return HierarchyToDataSet(BvhHierarchy.FromFileData(data));
        }

        // Transit BVH data struct to AnimDataSet
        public static AnimDataSet HierarchyToDataSet(BvhHierarchy hierarchy)
        {
            if (hierarchy.nodes.Count != indexMap.Length)
            {
                Debug.Log("Invalid node number: input " + hierarchy.nodes.Count + " indexMap " + indexMap.Length);
                return new AnimDataSet();
            }
            AnimDataSet dataSet = new AnimDataSet();
            dataSet.frameTime = hierarchy.frameTime;
            // Loop each frame
            foreach (List<float> numbers in hierarchy.frames)
            {
                if (numbers.Count != hierarchy.channelNodes.Count)
                {
                    Debug.Log("Invalid number of numbers");
                    return new AnimDataSet();
                }
                AnimData data = new AnimData();
                AnimUnitData unitData = new AnimUnitData();
                unitData.ResetJointAngles();
                unitData.id = 1;
                unitData.size = 1f;
                // Loop each node
                for (int i = 0; i < hierarchy.nodes.Count; i++)
                {
                    int opIndex = indexMap[i];
                    if (opIndex < 0) continue;
                    Vector3 pos = new Vector3();
                    Vector3 rot = new Vector3();
                    // Loop each channel
                    foreach (KeyValuePair<int, string> ch in hierarchy.nodes[i].channels)
                    {
                        float value = numbers[ch.Key];
                        switch (ch.Value)
                        {
                            case "Xposition": pos.x = value; break;
                            case "Yposition": pos.y = value; break;
                            case "Zposition": pos.z = value; break;
                            case "Xrotation": rot.x = value; break;
                            case "Yrotation": rot.y = value; break;
                            case "Zrotation": rot.z = value; break;
                            default: break;
                        }
                    }
                    if (opIndex == 0)
                    {
                        if (pos != new Vector3()) unitData.totalPosition = pos;
                        if (rot != new Vector3()) unitData.jointAngles[opIndex] = AnimUnitData.AdamToUnityEuler(rot);
                    } else
                    {
                        if (rot != new Vector3()) unitData.jointAngles[opIndex] = AnimUnitData.AdamToUnityEuler(rot);
                    }
                }
                //data.jointAngles[0] += Vector3.left * 180f;
                data.units.Add(unitData);
                dataSet.dataList.Add(data);
            }
            //Debug.Log(dataSet.dataLis);
            return dataSet;
        }
    }

    public class BvhHierarchy
    {
        public List<BvhNode> nodes = new List<BvhNode>();
        public Dictionary<int, BvhNode> channelNodes = new Dictionary<int, BvhNode>(); // channels and indices
        public int frameNumber;
        public float frameTime;
        private int channelIndex = 0;

        public List<List<float>> frames = new List<List<float>>();

        public static BvhHierarchy FromFileData(string fileData)
        {
            BvhHierarchy hierarchy = new BvhHierarchy();
            BvhNode currentNode = null;
            Section currentSection = Section.NONE;

            string[] lines = fileData.Split('\n');
            // Loop each line in file
            foreach (string str in lines)
            {                
                string str_noTab = Regex.Replace(str, @"\t", ""); // get rid of '\t'
                List<string> args = str_noTab.Split(' ').ToList();
                if (args.Count == 0) continue;
                try
                {
                    switch (args[0].Trim())
                    {
                        case "": break;

                        case "HIERARCHY": currentSection = Section.HIERARCHY; break;
                        case "MOTION": currentSection = Section.MOTION; break;

                        case "ROOT":
                            currentNode = hierarchy.AddNewNode(null, BvhNodeType.ROOT, args[1]);
                            break;
                        case "JOINT":
                            currentNode = currentNode.AddChild(hierarchy.AddNewNode(currentNode, BvhNodeType.JOINT, args[1]));
                            break;
                        case "End":
                            currentNode = currentNode.AddChild(hierarchy.AddNewNode(currentNode, BvhNodeType.ENDSITE, "_end"));
                            break;
                        case "OFFSET":
                            currentNode.SetOffset(args[1], args[2], args[3]);
                            break;
                        case "CHANNELS":
                            List<string> names = new List<string>(args);
                            names.RemoveRange(0, 2);
                            currentNode.SetChannels(args[1], names);
                            break;
                        case "{": break;
                        case "}":
                            if (currentNode.type != BvhNodeType.ROOT) currentNode = currentNode.parent;
                            break;

                        case "Frames:":
                            hierarchy.frameNumber = int.Parse(args[1]);
                            break;
                        case "Frame":
                            hierarchy.frameTime = float.Parse(args[2]);
                            break;

                        default:
                            // Number lines
                            if (currentSection == Section.MOTION)
                            {
                                if (!TryParsingNumbers(hierarchy, args))
                                {
                                    Debug.LogWarning("Grammar not desired: " + str_noTab);
                                }
                            } else
                            {
                                Debug.Log(args[0][9].ToString());
                                Debug.LogWarning("Grammar not desired: " + str_noTab);
                            }
                            break;
                    }
                }catch(Exception err)
                {
                    Debug.Log(err);
                    Debug.Log("Error in load BVH: in " + str_noTab);
                }
            }

            return hierarchy;
        }

        private static bool TryParsingNumbers(BvhHierarchy hierarchy, List<string> numbers)
        {
            int length = numbers.Count;
            if (length != hierarchy.channelNodes.Count)
            {
                Debug.LogWarning("Length not match");
                return false;
            }
            List<float> numberList = new List<float>();
            for (int i = 0; i < length; i++)
            {
                float num;
                if(float.TryParse(numbers[i], out num))
                {
                    numberList.Add(num);
                }
                else
                {
                    Debug.Log("Incorrect number format");
                    return false;
                }
            }
            hierarchy.frames.Add(numberList);
            return true;
        }

        public BvhNode AddNewNode(BvhNode parent, BvhNodeType type, string name)
        {
            BvhNode newNode = new BvhNode(this, parent, type, name);
            nodes.Add(newNode);
            return newNode;
        }

        public int RegisterChannels(int number, BvhNode node)
        {
            int startIndex = channelIndex;
            for (int i = 0; i < number; i++)
            {
                channelNodes.Add(channelIndex, node);
                channelIndex++;
            }
            return startIndex;
        }

        private enum Section
        {
            NONE,
            HIERARCHY,
            MOTION
        }
    }

    public class BvhNode
    {
        public BvhHierarchy hierarchy;
        public BvhNodeType type;
        public string name;
        public Vector3 offset;
        public Dictionary<int, string> channels = new Dictionary<int, string>(); // its own channels and indices (search in the hierarchy)
        public BvhNode parent;
        public List<BvhNode> children = new List<BvhNode>();

        public BvhNode(BvhHierarchy hierarchy, BvhNode parent, BvhNodeType type = BvhNodeType.NONE, string name = "my_node")
        {
            this.hierarchy = hierarchy;
            this.parent = parent;
            this.type = type;
            this.name = name;
        }

        public BvhNode AddChild(BvhNode childNode)
        {
            children.Add(childNode);
            return childNode;
        }

        public void SetOffset(string x, string y, string z)
        {
            offset = new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
        }

        public void SetChannels(string number, List<string> names)
        {
            int number_int = int.Parse(number);
            int startIndex = hierarchy.RegisterChannels(number_int, this);
            for (int i = 0; i < number_int; i++)
            {
                channels.Add(startIndex + i, names[i]);
            }
        }
    }

    public enum BvhNodeType
    {
        NONE,
        ROOT, 
        JOINT, 
        ENDSITE
    }
}
