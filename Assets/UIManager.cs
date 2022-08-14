using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CreateTree tree;

    public TMP_InputField inputField;
    public Slider slider;
    public TMP_Text sliderText;

    public void SliderUpdate()
    {
        sliderText.text = $"{slider.value}";
        tree.drawDepth = (int)slider.value;
        //DebugRenderer.DoDraw(true);
        //DebugRenderer.StartNewBatch();
        //tree.DrawInBuild();
        //DebugRenderer.DoDraw(false);
    }

    public void BodyUpdater()
    {
        tree.n = int.Parse(inputField.text);
    }
}
