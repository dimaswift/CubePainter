using UnityEngine;
using System.Collections.Generic;

namespace CubePainter
{
    public class ParticleMarker : Marker
    {
        public ParticleSystem part;
        List<ParticleCollisionEvent> m_collisionEvents = new List<ParticleCollisionEvent>();
        public bool emit;
        public bool useSafeCollision = true;
        Material mat;
        void Start()
        {
            part = GetComponent<ParticleSystem>();
            mat = part.GetComponent<Renderer>().sharedMaterial;
        }


        void FixedUpdate()
        {
            if (emit)
            {
                if (Input.GetMouseButton(0))
                {
                    mat.color = Decal.currentGradient;
                    part.Emit(1);
                    part.transform.position = HandyUtilities.Helper.GetMouse();
                }
                //transform.LookAt(Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(5), Vector3.up);
                //transform.position = Camera.main.transform.position + Camera.main.transform.TransformDirection(Vector3.forward) ;
            }

        }
        void OnParticleCollision(GameObject other)
        {
            var collCount = part.GetCollisionEvents(other, m_collisionEvents);
            var safe = part.GetSafeCollisionEventSize();
            Decal decal = other.GetComponent<Decal>();
            var count = useSafeCollision ? safe : collCount;
            if (decal == null) return;
         
            for (int i = 0; i < count; i++)
            {
                Mark(decal, m_collisionEvents[i].intersection);
            }
        }
    }

}
