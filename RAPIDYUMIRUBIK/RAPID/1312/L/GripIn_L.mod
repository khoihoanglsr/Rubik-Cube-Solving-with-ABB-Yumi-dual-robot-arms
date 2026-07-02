MODULE GripIn_L
    PROC close_gripper_L()
        WaitTime\InPos,0;
        g_GripIn \holdForce:= 14;
!        WaitTime\InPos,0;
    ENDPROC
ENDMODULE