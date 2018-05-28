using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]

public class RepeatTextureHorizontal : MonoBehaviour {

        SpriteRenderer floor;
        BoxCollider2D hitBox;

        void Awake()
        {

            floor = GetComponent<SpriteRenderer>();

            hitBox = GetComponent<BoxCollider2D>();


            Vector2 spriteSize = new Vector2(floor.bounds.size.x / transform.localScale.x, floor.bounds.size.y / transform.localScale.y);

            hitBox.size = spriteSize;

            GameObject childPrefab = new GameObject();
            SpriteRenderer childSprite = childPrefab.AddComponent<SpriteRenderer>();

            childPrefab.transform.position = transform.position;
            childSprite.sprite = floor.sprite;

            GameObject child;
            GameObject child2;

            for (int i = 1, l = (int)Mathf.Round(floor.bounds.size.x); i < l; i++)
            {

                child = Instantiate(childPrefab) as GameObject;
                child.transform.position = (transform.position - (new Vector3(spriteSize.x - (spriteSize.x / 2), 0, 0) * i));
                child.transform.parent = transform;

                child2 = Instantiate(childPrefab) as GameObject;
                child2.transform.position = (transform.position - (new Vector3(-spriteSize.x + (spriteSize.x / 2), 0, 0) * i));
                child2.transform.parent = transform;

            }


            childPrefab.transform.parent = transform;


            floor.enabled = false;
        }
    }
