using UnityEngine;

public class CustomButtonHandler : MonoBehaviour
{
    public experimentFlow flow;

    public void OnButtonClicked()
    {
        flow.SetNextScene("a_desktop");
        flow.LoadNextScene();
    }

    public void OnButtonEnter()
    {
        flow.SetNextScene("a_desktop_pointing");
        flow.LoadNextScene();
    }
}