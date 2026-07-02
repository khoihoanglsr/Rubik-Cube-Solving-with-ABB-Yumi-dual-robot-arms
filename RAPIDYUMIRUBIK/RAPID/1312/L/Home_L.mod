MODULE Home_L
    PROC p_home_L()
        AccSet 100,100,\FinePointRamp:=100;
        MoveJ home,v800,z100,Servo\WObj:=wobj0;
        g_GripIn \NoWait;
    ENDPROC
ENDMODULE