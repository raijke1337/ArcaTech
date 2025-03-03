using UnityEngine;

namespace ECM.Controllers
{
    public class NewPlayerInput : BaseCharacterController
    {
        [SerializeField] PlayerInputReaderObject _reader;

        public override void Awake()
        {
            base.Awake();
            _reader.EnablePlayerInputs();
        }
        protected override void HandleInput()
        {

            // Handle user input

            moveDirection = new Vector3
            {
                x = _reader.InputDirection.x,
                y = 0.0f,
                z = _reader.InputDirection.z
            };


    }



    }

}