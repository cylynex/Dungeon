using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetectText : MonoBehaviour {

    void Start() {
        GetComponent<InputField>().onEndEdit.AddListener(displayText);

    }

    private void displayText(string textInField) {
        print(textInField);
    }

}
