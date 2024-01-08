﻿using System;

namespace MonitorsPanel.Core.Manager
{
  [Flags]
  public enum SupportedHostingTargets
  {
    Monitors = 0b0000_0001,
    Publishers = 0b0000_0010
  }
}