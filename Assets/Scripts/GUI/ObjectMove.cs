using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour
{
    public GameObject[] objects; // 複数のゲームオブジェクトを管理
    public Color highlightColor = Color.yellow; // 選択中のオブジェクトの色
    public float moveSpeed = 5f; // 移動速度
    public float scaleSpeed = 0.1f; // スケール変更速度

    private int currentIndex = 0; // 現在選択されているオブジェクトのインデックス
    private Color[] originalColors; // 元のオブジェクトの色を保存

    void Start()
    {
        // 元の色を保存
        originalColors = new Color[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].TryGetComponent<Renderer>(out Renderer renderer))
            {
                originalColors[i] = renderer.material.color;
            }
        }

        // 初期選択の強調表示
        HighlightObject(currentIndex);
    }

    void Update()
    {
        // タブキーで選択切り替え
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchObject();
        }

        // 選択中のオブジェクトを移動
        MoveSelectedObject();

        // 選択中のオブジェクトのスケール変更
        ScaleSelectedObject();
    }

    void SwitchObject()
    {
        // 現在の選択をリセット
        ResetHighlight(currentIndex);

        // 次のオブジェクトに切り替え
        currentIndex = (currentIndex + 1) % objects.Length;

        // 新しい選択を強調表示
        HighlightObject(currentIndex);
    }

    void HighlightObject(int index)
    {
        if (objects[index].TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material.color = highlightColor;
        }
    }

    void ResetHighlight(int index)
    {
        if (objects[index].TryGetComponent<Renderer>(out Renderer renderer))
        {
            renderer.material.color = originalColors[index];
        }
    }

    void MoveSelectedObject()
    {
        if (objects[currentIndex] != null)
        {
            float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

            objects[currentIndex].transform.Translate(new Vector3(moveX, 0, moveZ));
        }
    }

    void ScaleSelectedObject()
    {
        if (objects[currentIndex] != null)
        {
            float scaleChange = 0;

            if (Input.GetKey(KeyCode.E))
            {
                scaleChange = scaleSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                scaleChange = -scaleSpeed * Time.deltaTime;
            }

            if (scaleChange != 0)
            {
                objects[currentIndex].transform.localScale += new Vector3(scaleChange, scaleChange, scaleChange);
            }
        }
    }
}
