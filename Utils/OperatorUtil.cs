using LuckyFlow.EnumDefine;

public class OperatorUtil {
    public static bool IsMatchedValue(long comparingOperator, long value, long comparingValue) {
        switch((COMPARING_OPERATOR)comparingOperator) {
            case COMPARING_OPERATOR.EQUAL: {
                if (value == comparingValue)
                    return true;
                return false;
            }
            case COMPARING_OPERATOR.LESS: {
                if (value < comparingValue)
                    return true;
                return false;
            }
            case COMPARING_OPERATOR.GREATER: {
                if (value > comparingValue)
                    return true;
                return false;
            }
            case COMPARING_OPERATOR.LESS_OR_EQUAL: {
                if (value <= comparingValue)
                    return true;
                return false;
            }
            case COMPARING_OPERATOR.GREATER_OR_EQUAL: {
                if (value >= comparingValue)
                    return true;
                return false;
            }
            case COMPARING_OPERATOR.NOT_EQUAL: {
                if (value != comparingValue)
                    return true;
                return false;
            }

            default:
                return false;
        }
    }
}