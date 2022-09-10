using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: For Test.
public class TestPositioning : MonoBehaviour
{
    public BoxCollider2D feetBox;
    public BoxCollider2D headBox;
    public BoxCollider2D bodyBox;

    public GameObject hObj;
    public GameObject hlObj;
    public GameObject hrObj;
    public GameObject fObj;
    public GameObject flObj;
    public GameObject frObj;
    public GameObject cObj;
    public GameObject clObj;
    public GameObject crObj;
    public GameObject ltObj;
    public GameObject lbObj;
    public GameObject rtObj;
    public GameObject rbObj;

    public float scale = 0.02f;

    protected Vector2 dir_hPos;
    protected Vector2 dir_hlPos;
    protected Vector2 dir_hrPos;
    protected Vector2 dir_fPos;
    protected Vector2 dir_flPos;
    protected Vector2 dir_frPos;
    protected Vector2 dir_cPos;
    protected Vector2 dir_clPos;
    protected Vector2 dir_crPos;
    protected Vector2 dir_ltPos;
    protected Vector2 dir_lbPos;
    protected Vector2 dir_rtPos;
    protected Vector2 dir_rbPos;

    protected Vector2 hPos; // Head Position
    protected Vector2 hlPos; // Head Left Position
    protected Vector2 hrPos; // Head Right Position
    protected Vector2 fPos; // Feet Position
    protected Vector2 flPos; // Feet Left Position
    protected Vector2 frPos; // Feet Right Position
    protected Vector2 cPos; // Center Position (Body Position)
    protected Vector2 clPos;
    protected Vector2 crPos;
    protected Vector2 ltPos; // Left Top Position
    protected Vector2 lbPos; // Left Bottom Position
    protected Vector2 rtPos; // Right Top Position
    protected Vector2 rbPos; // Right Bottom Position

    void Start()
    {
        m_InitPositions();
    }

    void FixedUpdate()
    {
        m_UpdatePositions();
    }

    private void m_InitPositions()
    {
        Bounds hBounds = headBox.bounds;
        Bounds fBounds = feetBox.bounds;
        Bounds cBounds = bodyBox.bounds;
        Vector2 pPos = transform.position;

        headBox.usedByComposite = false;
        feetBox.usedByComposite = false;
        bodyBox.usedByComposite = false;

        dir_hPos.Set(hBounds.center.x - pPos.x, hBounds.max.y - pPos.y);
        dir_hlPos.Set(hBounds.min.x - pPos.x, hBounds.center.y - pPos.y);
        dir_hrPos.Set(hBounds.max.x - pPos.x, hBounds.center.y - pPos.y);
        dir_fPos.Set(fBounds.center.x - pPos.x, fBounds.min.y - pPos.y);
        dir_flPos.Set(fBounds.min.x - pPos.x, fBounds.center.y - pPos.y);
        dir_frPos.Set(fBounds.max.x - pPos.x, fBounds.center.y - pPos.y);
        dir_cPos.Set(cBounds.center.x - pPos.x, cBounds.center.y - pPos.y);
        dir_clPos.Set(cBounds.min.x - pPos.x, cBounds.center.y - pPos.y);
        dir_crPos.Set(cBounds.max.x - pPos.x, cBounds.center.y - pPos.y);
        dir_ltPos.Set(hBounds.min.x - pPos.x, hBounds.min.y - pPos.y);
        dir_lbPos.Set(fBounds.min.x - pPos.x, fBounds.max.y - pPos.y);
        dir_rtPos.Set(hBounds.max.x - pPos.x, hBounds.min.y - pPos.y);
        dir_rbPos.Set(fBounds.max.x - pPos.x, fBounds.max.y - pPos.y);

        headBox.usedByComposite = true;
        feetBox.usedByComposite = true;
        bodyBox.usedByComposite = true;
    }

    private void m_UpdatePositions()
    {
        float z = -1.0f;

        Vector2 pPos = transform.position;

        hPos.Set(pPos.x + dir_hPos.x, pPos.y + dir_hPos.y);
        hlPos.Set(pPos.x + dir_hlPos.x, pPos.y + dir_hlPos.y);
        hrPos.Set(pPos.x + dir_hrPos.x, pPos.y + dir_hrPos.y);
        fPos.Set(pPos.x + dir_fPos.x, pPos.y + dir_fPos.y);
        flPos.Set(pPos.x + dir_flPos.x, pPos.y + dir_flPos.y);
        frPos.Set(pPos.x + dir_frPos.x, pPos.y + dir_frPos.y);
        cPos.Set(pPos.x + dir_cPos.x, pPos.y + dir_cPos.y);
        clPos.Set(pPos.x + dir_clPos.x, pPos.y + dir_clPos.y);
        crPos.Set(pPos.x + dir_crPos.x, pPos.y + dir_crPos.y);
        ltPos.Set(pPos.x + dir_ltPos.x, pPos.y + dir_ltPos.y);
        lbPos.Set(pPos.x + dir_lbPos.x, pPos.y + dir_lbPos.y);
        rtPos.Set(pPos.x + dir_rtPos.x, pPos.y + dir_rtPos.y);
        rbPos.Set(pPos.x + dir_rbPos.x, pPos.y + dir_rbPos.y);

        hObj.transform.position = (Vector3)hPos + Vector3.forward * z;
        hlObj.transform.position = (Vector3)hlPos + Vector3.forward * z;
        hrObj.transform.position = (Vector3)hrPos + Vector3.forward * z;
        fObj.transform.position = (Vector3)fPos + Vector3.forward * z;
        flObj.transform.position = (Vector3)flPos + Vector3.forward * z;
        frObj.transform.position = (Vector3)frPos + Vector3.forward * z;
        cObj.transform.position = (Vector3)cPos + Vector3.forward * z;
        clObj.transform.position = (Vector3)clPos + Vector3.forward * z;
        crObj.transform.position = (Vector3)crPos + Vector3.forward * z;
        ltObj.transform.position = (Vector3)ltPos + Vector3.forward * z;
        lbObj.transform.position = (Vector3)lbPos + Vector3.forward * z;
        rtObj.transform.position = (Vector3)rtPos + Vector3.forward * z;
        rbObj.transform.position = (Vector3)rbPos + Vector3.forward * z;

        hObj.transform.localScale = Vector3.one * scale;
        hlObj.transform.localScale = Vector3.one * scale;
        hrObj.transform.localScale = Vector3.one * scale;
        fObj.transform.localScale = Vector3.one * scale;
        flObj.transform.localScale = Vector3.one * scale;
        frObj.transform.localScale = Vector3.one * scale;
        cObj.transform.localScale = Vector3.one * scale;
        clObj.transform.localScale = Vector3.one * scale;
        crObj.transform.localScale = Vector3.one * scale;
        ltObj.transform.localScale = Vector3.one * scale;
        lbObj.transform.localScale = Vector3.one * scale;
        rtObj.transform.localScale = Vector3.one * scale;
        rbObj.transform.localScale = Vector3.one * scale;
    }
}
