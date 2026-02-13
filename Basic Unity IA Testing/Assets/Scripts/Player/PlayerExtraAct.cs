using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerExtraAct : MonoBehaviour
{
    [SerializeField] private Camera camA;
    [SerializeField] private Camera camB;

    private PlayerExtraActions _input;
    private CharacterController controller;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        camB.gameObject.SetActive(false);

        controller = GetComponent<CharacterController>();
        _input = new PlayerExtraActions();
        _input.Player.Enable();

        _input.Player.CamaraSwitch.performed += camaraSwitch_performed;
    }


    private void camaraSwitch_performed(InputAction.CallbackContext context)
    {
        camaraSwitch();
    }

private void camaraSwitch()
{
    if (camA.gameObject.activeSelf)
    {
        camA.gameObject.SetActive(false);
        camB.gameObject.SetActive(true);
    }
    else
    {
        camA.gameObject.SetActive(true);
        camB.gameObject.SetActive(false);
    }
}

}
