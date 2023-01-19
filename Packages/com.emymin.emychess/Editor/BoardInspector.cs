using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;
using System;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#endif

namespace Emychess{
    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    /// <summary>
    /// Inspector for the <see cref="Board"/> behaviour, allows to automatically generate the <see cref="VRCObjectPool"/> objects with the desired number of pieces
    /// </summary>
    [CustomEditor(typeof(Board))]
    public class BoardInspector : Editor
    {
        private SerializedProperty piece_pools;
        private SerializedProperty pieces_parent;
        private SerializedProperty whitePieceMaterial;
        private SerializedProperty blackPieceMaterial;
        private SerializedProperty ghostMaterial;
        private SerializedProperty currentRules;
        private SerializedProperty board_renderer;
        private SerializedProperty pieceMovedClip;
        private SerializedProperty pieceCapturedClip;
        private SerializedProperty chessManager;

        private SerializedProperty piecePrefabs;
        private SerializedProperty pieceAmounts;

        public struct piecePrefabAmount
        {
            public GameObject prefab;
            public int amount;
        }

        private void OnEnable()
        {
            piece_pools = serializedObject.FindProperty(nameof(Board.piece_pools));
            pieces_parent = serializedObject.FindProperty(nameof(Board.pieces_parent));
            whitePieceMaterial = serializedObject.FindProperty(nameof(Board.whitePieceMaterial));
            blackPieceMaterial = serializedObject.FindProperty(nameof(Board.blackPieceMaterial));
            ghostMaterial = serializedObject.FindProperty(nameof(Board.ghostMaterial));
            currentRules = serializedObject.FindProperty(nameof(Board.currentRules));
            board_renderer = serializedObject.FindProperty(nameof(Board.board_renderer));
            pieceMovedClip = serializedObject.FindProperty(nameof(Board.pieceMovedClip));
            pieceCapturedClip = serializedObject.FindProperty(nameof(Board.pieceCapturedClip));
            chessManager = serializedObject.FindProperty(nameof(Board.chessManager));


            piecePrefabs = serializedObject.FindProperty(nameof(Board.piecePrefabs));
            pieceAmounts = serializedObject.FindProperty(nameof(Board.pieceAmounts));

            Board board = (Board)target;
            if (board.pieceAmounts == null || board.pieceAmounts.Length < 6) { board.pieceAmounts = new[] { 16, 8, 2, 8, 16, 8 }; }
            if (board.piecePrefabs == null || board.piecePrefabs.Length < 6) { board.piecePrefabs = new GameObject[6]; }
        }

        public Dictionary<string,piecePrefabAmount> GetPieceDictionary(Board board)
        {
            Dictionary<string, piecePrefabAmount>  pieceDict = new Dictionary<string, piecePrefabAmount>();
            pieceDict["pawn"] = new piecePrefabAmount { prefab = board.piecePrefabs[0], amount = board.pieceAmounts[0] };
            pieceDict["bishop"] = new piecePrefabAmount { prefab = board.piecePrefabs[1], amount = board.pieceAmounts[1] };
            pieceDict["king"] = new piecePrefabAmount { prefab = board.piecePrefabs[2], amount = board.pieceAmounts[2] };
            pieceDict["knight"] = new piecePrefabAmount { prefab = board.piecePrefabs[3], amount = board.pieceAmounts[3] };
            pieceDict["queen"] = new piecePrefabAmount { prefab = board.piecePrefabs[4], amount = board.pieceAmounts[4] };
            pieceDict["rook"] = new piecePrefabAmount { prefab = board.piecePrefabs[5], amount = board.pieceAmounts[5] };
            return pieceDict;
        }

        public void GeneratePools(Board board, Dictionary<string, piecePrefabAmount> pieceDict=null)
        {
            
            if (pieceDict == null)
            {
                pieceDict = GetPieceDictionary(board);

            }
            foreach (VRCObjectPool pool in board.piece_pools)
            {
                for (int i = pool.transform.childCount; i > 0; i--)
                {
                    DestroyImmediate(pool.transform.GetChild(0).gameObject);
                }
                piecePrefabAmount ppa = pieceDict[pool.name];
                pool.Pool = new GameObject[ppa.amount];

                for (int i = 0; i < ppa.amount; i++)
                {
                    GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(ppa.prefab, pool.transform);
                    //PrefabUtility.UnpackPrefabInstance(spawned,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
                    spawned.name = pool.name + i;
                    pool.Pool[i] = spawned;
                    Piece piece = spawned.GetUdonSharpComponent<Piece>();
                    piece.UpdateProxy();
                    piece.board = board;
                    piece.type = pool.name;
                    piece.pool = pool;
                    piece.ApplyProxyModifications();
                    spawned.SetActive(false);
                    UnityEditor.EditorUtility.SetDirty(spawned);
                }
                UnityEditor.EditorUtility.SetDirty(pool);
                
            }
            UnityEditor.EditorUtility.SetDirty(PrefabUtility.GetOutermostPrefabInstanceRoot(board.gameObject));
            //PrefabUtility.ApplyPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(board.gameObject), InteractionMode.AutomatedAction);
            Debug.Log("[EmyChess]: Pools succesfully generated");
        }
        public void ResetPools(Board board)
        {
            foreach (VRCObjectPool pool in board.piece_pools)
            {
                for (int i = pool.transform.childCount; i > 0; i--)
                {
                    DestroyImmediate(pool.transform.GetChild(0).gameObject);
                }
                pool.Pool = null;
            }
            Debug.Log("[EmyChess]: Pools succesfully resetted",board);
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            Board board = (Board)target;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pieces_parent);
            EditorGUILayout.PropertyField(whitePieceMaterial);
            EditorGUILayout.PropertyField(blackPieceMaterial);
            EditorGUILayout.PropertyField(ghostMaterial);
            EditorGUILayout.PropertyField(board_renderer);
            EditorGUILayout.PropertyField(pieceMovedClip);
            EditorGUILayout.PropertyField(pieceCapturedClip);
            EditorGUILayout.PropertyField(chessManager);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rules Object (currently only supports default, do not switch)");
            EditorGUILayout.PropertyField(currentRules);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(piece_pools, new GUIContent("Piece Pools"), true);


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Piece Prefabs");
            int i = 0;
            foreach (KeyValuePair<string,piecePrefabAmount> keyValue in GetPieceDictionary(board))
            {
                string name = keyValue.Key;
                EditorGUILayout.LabelField(char.ToUpper(name[0]) + name.Substring(1));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(piecePrefabs.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUILayout.PropertyField(pieceAmounts.GetArrayElementAtIndex(i), GUIContent.none);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                i++;
            }


            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("WARNING, generating/resetting pools doesn't work on prefab instances");
            EditorGUILayout.LabelField("Unpack the prefab FIRST or run inside the prefab");

            if (GUILayout.Button("Generate Pools"))
            {
                if (PrefabUtility.GetPrefabInstanceStatus(board.piece_pools[0].gameObject) != PrefabInstanceStatus.NotAPrefab)
                {
                    Debug.LogError("Pools are in a prefab instance, unpack FIRST or run inside the prefab instead",board);
                }
                else
                {
                    GeneratePools(board);
                }
                
            }

            if(GUILayout.Button("Reset Pools"))
            {
                if (PrefabUtility.GetPrefabInstanceStatus(board.piece_pools[0].gameObject) != PrefabInstanceStatus.NotAPrefab)
                {
                    Debug.LogError("Pools are in a prefab instance, unpack FIRST or run inside the prefab instead", board);
                }
                else
                {
                    ResetPools(board);
                }
            }
        
        }
    }
    #endif
}