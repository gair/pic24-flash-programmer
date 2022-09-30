namespace PIC24FlashProgrammer
{
    internal class Animation
    {
        public static async Task AnimateControl(Control ctrl, int start, int finish, int speed, int delay, string propertyName)
        {
            await Task.Run(() =>
            {
                var distance = Math.Abs(finish - start);
                if (distance == 0)
                {
                    return;
                }

                var scale = Math.PI / 2 / distance;
                var direction = (start > finish) ? -1 : 1;

                int i;
                for (i = 0; i <= distance; i += speed)
                {
                    Thread.Sleep(delay);
                    var value = start + (int)(direction * distance * Math.Abs(Math.Sin(i * scale)));
                    AdjustControl(ctrl, propertyName, value);
                }

                if (i != distance)
                {
                    Thread.Sleep(delay);
                    AdjustControl(ctrl, propertyName, finish);
                }
            });
        }

        private static void AdjustControl(Control ctrl, string propertyName, object value)
        {
            if (ctrl.InvokeRequired)
            {
                _ = ctrl.BeginInvoke(new Action<Control, string, object>(AdjustControl), ctrl, propertyName, value);
                return;
            }

            var pi = ctrl.GetType().GetProperty(propertyName);
            pi?.SetValue(ctrl, value, null);
        }
    }
}
