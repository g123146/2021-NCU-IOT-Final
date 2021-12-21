using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using System;

public class MapController : MonoBehaviour
{
    [SerializeField] TMP_Dropdown anchor_LT = null;
    [SerializeField] TMP_Dropdown anchor_LB = null;
    [SerializeField] TMP_Dropdown anchor_RT = null;
    [SerializeField] TMP_Dropdown anchor_RB = null;

    [SerializeField] TMP_InputField widthInput = null;
    [SerializeField] TMP_InputField lengthInput = null;

    [SerializeField] RectTransform tagRect = null;

    RectTransform mapRect;
    Vector2 mapWL;

    float velosity = 10;
    
    float _width;
    float width
    {
        get { return _width; }
        set
        {
            _width = value;
        }
    }
    float _length;
    float length
    {
        get { return _length; }
        set
        {
            _length = value;
        }
    }
    enum Base
    {        
        Base1,
        Base2,
        Base3,
        None,
    }
    enum Dimention
    {
        Width,
        Length,
    }
    Dictionary<Base, TMP_Dropdown> base2Dropdown = new Dictionary<Base, TMP_Dropdown>();
    Dictionary<TMP_Dropdown, Base> dropdown2base => base2Dropdown.ToDictionary(x => x.Value, x => x.Key);

    Dictionary<TMP_InputField, Dimention> inputField2Dimention = new Dictionary<TMP_InputField, Dimention>();
    public void Initialize()
    {
        mapRect = transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform parent = GetComponent<RectTransform>();
        mapWL.x = (mapRect.anchorMax.x - mapRect.anchorMin.x) *parent.rect.width + mapRect.sizeDelta.x;
        mapWL.y = (mapRect.anchorMax.y - mapRect.anchorMin.y) *parent.rect.height + mapRect.sizeDelta.y;
        print($"X : {mapWL.x}, Y : {mapWL.y}");
        width = float.Parse(widthInput.text);
        length = float.Parse(lengthInput.text);

        anchor_LT.value = (int)Base.Base1;
        base2Dropdown[Base.Base1] = anchor_LT;
        anchor_LB.value = (int)Base.Base2;
        base2Dropdown[Base.Base2] = anchor_LB;
        anchor_RT.value = (int)Base.Base3;
        base2Dropdown[Base.Base3] = anchor_RT;
        anchor_RB.value = (int)Base.None;
        base2Dropdown[Base.None] = anchor_RB;
        anchor_LT.onValueChanged.AddListener((value) => { DropdownOnvalueChange(anchor_LT, value); });
        anchor_LB.onValueChanged.AddListener((value) => { DropdownOnvalueChange(anchor_LB, value); });
        anchor_RT.onValueChanged.AddListener((value) => { DropdownOnvalueChange(anchor_RT, value); });
        anchor_RB.onValueChanged.AddListener((value) => { DropdownOnvalueChange(anchor_RB, value); });

        inputField2Dimention[widthInput] = Dimention.Width;
        inputField2Dimention[lengthInput] = Dimention.Length;
        widthInput.onEndEdit.AddListener((value) => { InputOnEndEdit(widthInput, value); });
        lengthInput.onEndEdit.AddListener((value) => { InputOnEndEdit(lengthInput, value); });
    }
    void DropdownOnvalueChange(TMP_Dropdown dropdown,int value)
    {
        var otherDropdown = base2Dropdown[(Base)value];
        if(otherDropdown!= dropdown)
        {
            var oldValue = dropdown2base[dropdown];
            otherDropdown.value = (int)oldValue;
            base2Dropdown[oldValue] = otherDropdown;
            base2Dropdown[(Base)value] = dropdown;
        }
        
    }
    void InputOnEndEdit(TMP_InputField inputField, string str)
    {
        switch (inputField2Dimention[inputField])
        {
            case Dimention.Length:
                length = float.Parse(str);
                break;
            case Dimention.Width:
                width = float.Parse(str);
                break;
        }
    }
    void UpdateMap()
    {
        
    }
    public void UpdateTagPosition(float base1, float base2, float base3)
    {
        List<Point> points = new List<Point>();
        if (anchor_LT.value < 3)
        {
            double distance = GetBaseDistance(anchor_LT.value, base1, base2, base3);
            Point p = new Point() { X = 0, Y = length, Distance = distance };
            points.Add(p);
        }
        if (anchor_LB.value < 3)
        {
            double distance = GetBaseDistance(anchor_LB.value, base1, base2, base3);
            Point p = new Point() { X = 0, Y = 0, Distance = distance };
            points.Add(p);
        }
        if (anchor_RT.value < 3)
        {
            double distance = GetBaseDistance(anchor_RT.value, base1, base2, base3);
            Point p = new Point() { X = width, Y = length, Distance = distance };
            points.Add(p);
        }
        if (anchor_RB.value < 3)
        {
            double distance = GetBaseDistance(anchor_RB.value, base1, base2, base3);
            Point p = new Point() { X = width, Y = 0, Distance = distance };
            points.Add(p);
        }
        var pTag = GetPiontByThree(points);
        //print($"X : {pTag.X}, Y : {pTag.Y}");
        var x = mapWL.x * pTag.X / width;
        var y = mapWL.y * pTag.Y / length;
        //print($"X : {x}, Y : {y}");
        var vector = new Vector2((float)x, (float)y) - tagRect.anchoredPosition;
        //tagRect.anchoredPosition = new Vector2((float)x, (float)y);
        tagRect.anchoredPosition += vector * velosity * Time.deltaTime;
        
    }
    double GetBaseDistance(int value, float base1, float base2, float base3)
    {
        double distance = 0;
        switch ((Base)value)
        {
            case Base.Base1:
                distance = base1;
                break;
            case Base.Base2:
                distance = base2;
                break;
            case Base.Base3:
                distance = base3;
                break;
        }
        return distance;
    } 
    /// <summary>
    /// 三点绝对定位
    /// </summary>
    Point GetPiontByThree(List<Point> points)
    {
        /* Math.Pow(y1-Y)+Math.Pow(X-x1)=Math.Pow(D1)
         * Math.Pow(y2-Y)+Math.Pow(X-x2)=Math.Pow(D2)
         * Math.Pow(y3-Y)+Math.Pow(X-x3)=Math.Pow(D3)
         * 1-3.2-3解得：
         * 2 * (p1.X - p3.X)x + 2 * (p1.Y - p3.Y)y = Math.Pow(p1.X, 2) - Math.Pow(p3.X, 2) + Math.Pow(p1.Y, 2) - Math.Pow(p3.Y, 2) + Math.Pow(p3.Distance, 2) - Math.Pow(p1.Distance, 2);
         * 2 * (p2.X - p3.X)x + 2 * (p2.Y - p3.Y)y = Math.Pow(p2.X, 2) - Math.Pow(p3.X, 2) + Math.Pow(p2.Y, 2) - Math.Pow(p3.Y, 2) + Math.Pow(p3.Distance, 2) - Math.Pow(p2.Distance, 2);
         * 简化：
         * 2Ax+2By=C
         * 2Dx+2Ey=F
         * 简化：
         * x=(BF-EC)/(2BD-2AE)
         * y=(AF-DC)/(2AE-2BD)
         */
        var A = points[0].X - points[2].X;
        var B = points[0].Y - points[2].Y;
        var C = Math.Pow(points[0].X, 2) - Math.Pow(points[2].X, 2) + Math.Pow(points[0].Y, 2) - Math.Pow(points[2].Y, 2) + Math.Pow(points[2].Distance, 2) - Math.Pow(points[0].Distance, 2);
        var D = points[1].X - points[2].X;
        var E = points[1].Y - points[2].Y;
        var F = Math.Pow(points[1].X, 2) - Math.Pow(points[2].X, 2) + Math.Pow(points[1].Y, 2) - Math.Pow(points[2].Y, 2) + Math.Pow(points[2].Distance, 2) - Math.Pow(points[1].Distance, 2);

        var x = (B * F - E * C) / (2 * B * D - 2 * A * E);
        var y = (A * F - D * C) / (2 * A * E - 2 * B * D);

        Point P = new Point() { X = x, Y = y, Distance = 0 };
        return P;
    }
}
public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
    //表示指定点，据此点的距离
    public double Distance { get; set; }
}
