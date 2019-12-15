using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Isometric
{
    public interface IISOBasis
    {
        ISOBasis GetISOBasis();

        void SetUp(ISOBasis basis = null);
        void Remove();
        void DestroyISOBasis();

        Bounds GetBounds();

        bool IsOnGroundObject();

        void Update_SortingOrder(bool bChildRC);
        int CalcSortingOrder(bool bWithBasis);
    }
}