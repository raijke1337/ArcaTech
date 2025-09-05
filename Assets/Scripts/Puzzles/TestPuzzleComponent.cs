namespace Arcatech.Puzzles
{
    public class TestPuzzleComponent : BasePuzzleComponent
    {

        public void ButtonWasClicked(bool ok)
        {
            ResultCallback(ok);
        }

        protected override void SetUpPuzzle()
        {
            
        }
    }
}