using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceMaterial : MonoBehaviour
{
    /// <summary>
    /// Top speed the player can move on this surface
    /// </summary>
    public GameController.SurfaceSpeeds surfaceSpeeds;

    /// <summary>
    /// Whether or not the player can change the material of this surface
    /// </summary>
    public bool changeable;

    /// <summary>
    /// Type of material on this surface
    /// </summary>
    public GameController.material type;

    /// <summary>
    /// Reference to player
    /// </summary>
    protected Player player;

    
    void Start()
    {
        SetTiling();
        InitializeSurfaceSpeeds(type);
        player = GameObject.FindWithTag("GameController").GetComponent<GameController>().player;
    }

    /// <summary>
    /// Configures material to tile according to quad scale
    /// </summary>
    void SetTiling()
    {
        GetComponent<Renderer>().material.mainTextureScale = transform.localScale;
    }

    /// <summary>
    /// Initializes surface with associated move speeds from surfaceSpeeds (called from derived class)
    /// </summary>
    protected void InitializeSurfaceSpeeds(GameController.material materialType)
    {
        // Initialize surface speeds
		surfaceSpeeds = GameObject.FindWithTag("GameController").GetComponent<GameController>().speedMapping[materialType];
    }

    /// <summary>
    /// Right click reverts material to original color, left click assigns it the player's equipped color
    /// </summary>
    void OnMouseOver()
    {
        if(changeable) {
            // Left click
            if (Input.GetMouseButton(0))
            {
                ChangeMaterial();
            }
            // Right click
            else if (Input.GetMouseButton(1))
            {
                GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Changes the appearance of the material (currently just changes color)
    /// </summary>
    void ChangeMaterial()
    {
        switch(player.equippedMaterial) {
            case GameController.material.BOUNCE:
                GetComponent<Renderer>().material.color = Color.blue;
                break;
            case GameController.material.SLIP:
                GetComponent<Renderer>().material.color = Color.red;
                break;
            case GameController.material.STICK:
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
        }
        type = player.equippedMaterial;

        InitializeSurfaceSpeeds(player.equippedMaterial);
    }

}